using System;

namespace ConnectFourAI;

public class MoveCache
{
    private static Dictionary<string, ReturnMove> boardCache = new Dictionary<string, ReturnMove>();

    //Hash function
    public static string HashBoard(Board board)
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

    //Reverse the board, e.g. 1 -> 2, 2 -> 1
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

    //Saves the move to the file system's cache file
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

    //Reads the cache file and loads the moves into the dictionary
    public static void LoadCache()
    {
        try
        {
            using (StreamReader sr = new StreamReader("cache.txt"))
            {
                string? line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    //Ignores comments
                    if (line.StartsWith("#")) continue;

                    string[] split = line.Split(',');
                    boardCache.Add(split[0], new ReturnMove(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])));
                }
            }
        }
        catch (Exception e)
        {
            cText.WriteLine(e.Message);
        }

        //Order by the number of 0 in the board
        boardCache = boardCache.OrderByDescending(x => x.Key.Count(c => c == '0')).ToDictionary(x => x.Key, x => x.Value);
    }

    public static void AddToCache(string board, ReturnMove move)
    {
        if (boardCache.ContainsKey(board)) return;
        boardCache.Add(board, move);
        AppendToFile(board, move);
        cText.WriteLine("Appended to cache", "CACHE", ConsoleColor.Green);
    }

    //Saves the move to the dictionary and the file system
    public static void AddToCache(Board board, ReturnMove move)
    {
        AddToCache(HashBoard(board), move);
    }

    //Checks if the move is in the dictionary
    public static ReturnMove CacheLookup(Board board)
    {
        //If cache is empty, load it
        if (boardCache.Count == 0) LoadCache();

        ReturnMove returnMove = new ReturnMove(-1, 0, 0);
        string hash = HashBoard(board), reverseHash = HashBoard(ReverseBoard(board));

        Parallel.ForEach(boardCache, (move) =>
        {
            if (move.Key == hash || move.Key == reverseHash)
            {
                //Check if the move is valid
                if (board.ValidMove(move.Value.Column))
                {
                    returnMove = move.Value;
                    return;
                }   
            }
        });

        return returnMove;
    }
}