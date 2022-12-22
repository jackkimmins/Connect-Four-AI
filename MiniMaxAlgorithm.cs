using System;

namespace ConnectFourAI;

public class MoveCache
{
    private static Dictionary<string, ReturnMove> boardCache = new Dictionary<string, ReturnMove>();

    //Hash function
    private static string HashBoard(Board board)
    {
        string hash = "";
        for (int i = 0; i < board.NUM_ROW; i++)
        {
            for (int j = 0; j < board.NUM_COL; j++)
            {
                hash += board.board[i, j];
            }
        }

        return hash;
    }

    private static Board ReverseBoard(Board board)
    {
        Board newBoard = new Board(board);
        for (int i = 0; i < board.NUM_ROW; i++)
        {
            for (int j = 0; j < board.NUM_COL; j++)
            {
                if (board.board[i, j] == 1) newBoard.board[i, j] = 2;
                else if (board.board[i, j] == 2) newBoard.board[i, j] = 1;
            }
        }

        return newBoard;
    }

    private static bool AppendToFile(string board, ReturnMove move)
    {
        try
        {
            using (StreamWriter sw = File.AppendText("cache.txt"))
            {
                sw.WriteLine(board + "," + move.Column + "," + move.Score + "," + move.Iterations);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public static void LoadCache()
    {
        try
        {
            using (StreamReader sr = new StreamReader("cache.txt"))
            {
                string? line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] split = line.Split(',');
                    boardCache.Add(split[0], new ReturnMove(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void AddBoard(Board board, ReturnMove move)
    {
        if (boardCache.ContainsKey(HashBoard(board))) return;
        boardCache.Add(HashBoard(board), move);
        AppendToFile(HashBoard(board), move);
        cText.WriteLine("Appended to cache");
    }

    public static ReturnMove CacheLookup(Board board)
    {
        //if cache is empty, load it
        if (boardCache.Count == 0) LoadCache();

        if (boardCache.ContainsKey(HashBoard(board))) return boardCache[HashBoard(board)];
        else if (boardCache.ContainsKey(HashBoard(ReverseBoard(board))))
        {
            ReturnMove move = boardCache[HashBoard(ReverseBoard(board))];
            return new ReturnMove(move.Column, -move.Score, move.Iterations);
        }
        else return new ReturnMove(-1, 0, 0);
    }
}

public struct ReturnMove
{
    public int Iterations;
    public int Column;
    public int Score;

    public ReturnMove(int column, int score, int iterations)
    {
        Column = column;
        Score = score;
        Iterations = iterations;
    }
}

public class MiniMaxAlgorithm
{
    public int DEPTH { get; set; }
    public bool DebugMode { get; set; }
    public int iterations { get; set; } = 0;

    public MiniMaxAlgorithm(int depth = 6, bool debug = false)
    {
        DEPTH = depth;
        DebugMode = debug;
    }

   private List<int> ValidMoves(Board board)
    {
        return Enumerable.Range(1, board.NUM_COL)
                         .Where(i => board.board[0, i - 1] == 0)
                         .ToList();
    }

    private bool IsTerminal(Board board)
    {
        return board.WinningMove(board) != 0 || board.IsFull();
    }

    private void ConditionalScoreAjdustment(ref int player1, ref int player2, ref int score)
    {
        if (player1 == 4) score += 100;
        else if (player2 == 4) score -= 100;
        else if (player1 == 3 && player2 == 0) score += 5;
        else if (player2 == 3 && player1 == 0) score -= 5;
        else if (player1 == 2 && player2 == 0) score += 2;
        else if (player2 == 2 && player1 == 0) score -= 2;
    }

    private int Evaluate(Board board)
    {
        int score = 0;

        //Check horizontal
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int player1 = 0, player2 = 0;

                for (int k = 0; k < 4; k++)
                {
                    if (board.board[i, j + k] == 1) player1++;
                    else if (board.board[i, j + k] == 2) player2++;
                }

                ConditionalScoreAjdustment(ref player1, ref player2, ref score);
            }
        }

        //Check vertical
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                int player1 = 0, player2 = 0;

                for (int k = 0; k < 4; k++)
                {
                    if (board.board[i + k, j] == 1) player1++;
                    else if (board.board[i + k, j] == 2) player2++;
                }

                ConditionalScoreAjdustment(ref player1, ref player2, ref score);
            }
        }

        //Check diagonal (top left to bottom right)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int player1 = 0, player2 = 0;

                for (int k = 0; k < 4; k++)
                {
                    if (board.board[i + k, j + k] == 1) player1++;
                    else if (board.board[i + k, j + k] == 2) player2++;
                }

                ConditionalScoreAjdustment(ref player1, ref player2, ref score);
            }
        }

        //Check diagonal (top right to bottom left)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 3; j < 7; j++)
            {
                int player1 = 0, player2 = 0;

                for (int k = 0; k < 4; k++)
                {
                    if (board.board[i + k, j - k] == 1) player1++;
                    else if (board.board[i + k, j - k] == 2) player2++;
                }

                ConditionalScoreAjdustment(ref player1, ref player2, ref score);
            }
        }

        return score;
    }

    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        iterations++;

        if (depth == 0 || IsTerminal(board)) return Evaluate(board);

        List<int> validMoves = ValidMoves(board);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (int move in validMoves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, false);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (int move in validMoves)
            {
                Board newBoard = new Board(board);
                newBoard.MakeMove(move);
                int eval = Minimax(newBoard, depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }

            return minEval;
        }
    }

    //Checks if the current player has a winning move, if so, return that move
    public ReturnMove GetQuickMove(Board board)
    {
        int winningMove = board.NextWinningMove(board);

        if (winningMove != -1)
        {
            if (DebugMode) cText.WriteLine("Quick Search Move Found", "DEBUG", ConsoleColor.Green);
            return new ReturnMove(winningMove, 100, winningMove);
        }

        return new ReturnMove(-1, 0, 0);
    }

    public ReturnMove GetBestMove(Board board)
    {
        iterations = 0;
        int bestScore = int.MinValue;
        int bestMove = 0;

        if (DebugMode) Console.WriteLine("Evaluating moves...");

        ReturnMove quickMove = GetQuickMove(board);
        if (quickMove.Column != -1) return quickMove;

        //Check the cache for the cached board and the reverse of the board
        ReturnMove cachedMove = MoveCache.CacheLookup(board);
        if (cachedMove.Column != -1)
        {
            if (DebugMode) cText.WriteLine("Cache Hit!", "DEBUG", ConsoleColor.Green);
            return cachedMove;
        }

        Parallel.ForEach(ValidMoves(board), move =>
        {
            Board newBoard = new Board(board);
            newBoard.MakeMove(move, 1);
            int score = Minimax(newBoard, DEPTH, int.MinValue, int.MaxValue, false);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        });

        if (DebugMode)
        {
            Console.WriteLine($"Best move: {bestMove} with score: {bestScore}\nOut of {iterations.ToString("N0")} iterations.");
            // Thread.Sleep(2000);
        }

        ReturnMove returnMove = new ReturnMove(bestMove, bestScore, iterations);

        MoveCache.AddBoard(board, returnMove);

        return returnMove;
    }
}