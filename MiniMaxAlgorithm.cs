using System;

namespace ConnectFourAI;

public struct ReturnMove
{
    public int Iterations;
    public int Column;
    public int Score;
    public int TransSize;

    public ReturnMove(int column, int score, int iterations, int transSize = 0)
    {
        Column = column;
        Score = score;
        Iterations = iterations;
        TransSize = transSize;
    }
}

public class MiniMaxAlgorithm
{
    public int DEPTH { get; set; }
    public bool DebugMode { get; set; }
    public bool UseCache { get; set; }
    public int iterations { get; set; } = 0;
    private TranspositionTable transpositionTable = new TranspositionTable();

    //Constructor
    public MiniMaxAlgorithm(int depth = 6, bool debug = false, bool useCache = true)
    {
        DEPTH = depth;
        DebugMode = debug;
        UseCache = useCache;
    }

    //Gets a list of the columns that are not full
    private List<int> ValidMoves(ref Board board)
    {
        List<int> validMoves = new List<int>();

        for (int i = 0; i < board.NUM_COL; i++)
        {
            if (board.board[0, i] == 0) validMoves.Add(i + 1);
        }

        return validMoves;
    }

    //Returns if the game is over for a given board
    private bool IsTerminal(Board board)
    {
        return board.WinningMove(board) != 0 || board.IsFull();
    }

    //Adjusts the score based on points scored by both players
    private void ConditionalScoreAjdustment(ref int player1, ref int player2, ref int score)
    {
        if (player1 == 4) score += 105;
        else if (player1 == 3 && player2 == 0) score += 5;
        else if (player1 == 2 && player2 == 0) score += 2;
        else if (player1 == 1 && player2 == 0) score += 1;
        else if (player2 == 4) score -= 105;
        else if (player2 == 3 && player1 == 0) score -= 5;
        else if (player2 == 2 && player1 == 0) score -= 2;
        else if (player2 == 1 && player1 == 0) score -= 1;
    }

    //Evaluates the board and returns a score
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

    //Checks if the current player has a winning move, if so, return that move
    public ReturnMove GetQuickMove(Board board)
    {
        int winningMove = board.NextWinningMove(board);

        if (winningMove != -1) return new ReturnMove(winningMove, 100, winningMove);
        return new ReturnMove(-1, 0, 0);
    }

    //Determines if the board is in a quiet state
    private bool IsQuiet(ref Board board)
    {
        if (board.OpenFourInARowLines() > 0) return false;
        if (board.OpenThreeInARowLines() > 0) return false;

        return true;
    }

    //Using the minimax algorithm, alpha-beta pruning, and transposition tables, determine the best move.
    private int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        iterations++;

        if (depth == 0 || IsTerminal(board) || IsQuiet(ref board))
        {
            return Evaluate(board);
        }

        ReturnMove quickMove = GetQuickMove(board);
        if (quickMove.Column != -1)
        {
            return quickMove.Score;
        }

        int bestScore = maximizingPlayer ? int.MinValue : int.MaxValue;

        List<int> validMoves = ValidMoves(ref board);

        foreach (int move in validMoves)
        {
            Board newBoard = new Board(board);
            newBoard.MakeMove(move, maximizingPlayer ? 1 : 2);

            int score = 0;

            ulong key = newBoard.GetHash();

            if (transpositionTable.Get(key) != 0)
            {
                score = transpositionTable.Get(key);
            }
            else
            {
                score = Minimax(newBoard, depth - 1, alpha, beta, !maximizingPlayer);
                transpositionTable.Put(key, score);
            }

            if (maximizingPlayer)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    alpha = Math.Max(alpha, bestScore);
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    beta = Math.Min(beta, bestScore);
                }
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        return bestScore;
    }

    //Returns the best move for the current player
    public ReturnMove GetBestMove(Board board)
    {
        iterations = 0;
        int bestScore = int.MinValue;
        int bestMove = 0;

        if (DebugMode) cText.WriteLine("Evaluating board state...", "DEBUG", ConsoleColor.Green);

        ReturnMove quickMove = GetQuickMove(board);
        if (quickMove.Column != -1) return quickMove;

        //Check the cache for the cached board and the reverse of the board
        if (UseCache)
        {
            ReturnMove cachedMove = MoveCache.CacheLookup(board);
            if (cachedMove.Column != -1)
            {
                if (DebugMode) cText.WriteLine("Cache Hit! Col: " + cachedMove.Column, "DEBUG", ConsoleColor.Green);

                // Thread.Sleep(new Random().Next(1000, 3000));

                return cachedMove;
            }
        }

        Parallel.ForEach(ValidMoves(ref board), move =>
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

        if (DebugMode) cText.WriteLine($"Best move: {bestMove} with score: {bestScore} Out of {iterations.ToString("N0")} iterations.", "DEBUG", ConsoleColor.Green);

        ReturnMove returnMove = new ReturnMove(bestMove, bestScore, iterations, transpositionTable.Collisions);

        // if (UseCache) Task.Run(() => MoveCache.AddToCache(board, returnMove));
        // transpositionTable.Reset();

        return returnMove;
    }
}