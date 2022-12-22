using System;

namespace ConnectFourAI;

public class Board
{
    public int[,] board = new int[6, 7];
    public int currentPlayerTurn = 1;
    public int moves = 0;

    public int NUM_COL
    {
        get => board.GetLength(1);
    }

    public int NUM_ROW
    {
        get => board.GetLength(0);
    }

    public Board() {}

    public Board(Board b)
    {
        for (int i = 0; i < NUM_ROW; i++)
        {
            for (int j = 0; j < NUM_COL; j++) board[i, j] = b.board[i, j];
        }

        currentPlayerTurn = b.currentPlayerTurn;
        moves = b.moves;
    }

    public bool IsFull() => moves == 42;

    public void PrintBoard(bool shouldClear = false)
    {
        if (shouldClear) Console.Clear();

        Console.WriteLine(" 1 2 3 4 5 6 7");
        for (int i = 0; i < 6; i++)
        {
            Console.Write("|");
            for (int j = 0; j < 7; j++)
            {
                if (board[i, j] == 0) Console.Write(" |");
                else if (board[i, j] == 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("X");
                    Console.ResetColor();
                    Console.Write("|");
                }
                else if (board[i, j] == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("O");
                    Console.ResetColor();
                    Console.Write("|");
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine("---------------");
    }

    //Returns the board as a 2d array
    public List<List<int>> GetBoard()
    {
        List<List<int>> boardList = new List<List<int>>();
        for (int i = 0; i < NUM_ROW; i++)
        {
            List<int> row = new List<int>();
            for (int j = 0; j < NUM_COL; j++) row.Add(board[i, j]);
            boardList.Add(row);
        }
        return boardList;
    }

    public ulong GetHash()
    {
        ulong hash = 0;
        for (int i = 0; i < NUM_ROW; i++)
        {
            for (int j = 0; j < NUM_COL; j++)
            {
                hash = hash * 3 + (ulong) board[i, j];
            }
        }
        return hash;
    }

    public bool MakeMove(int column, int player)
    {
        if (column < 1 || column > 7) return false;
        if (board[0, column - 1] != 0) return false;

        for (int i = 5; i >= 0; i--)
        {
            if (board[i, column - 1] == 0)
            {
                board[i, column - 1] = player;
                break;
            }
        }

        moves++;
        currentPlayerTurn = (currentPlayerTurn == 1) ? 2 : 1;
        return true;
    }

    public bool MakeMove(int column) => MakeMove(column, currentPlayerTurn);

    //Find the next winning move and returns the column, if no winning move is found, returns 0. If it is a draw, returns -1
    public int WinningMove(Board board)
    {
        //Check Horizontal
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board.board[i, j] != 0 &&
                    board.board[i, j] == board.board[i, j + 1] &&
                    board.board[i, j] == board.board[i, j + 2] &&
                    board.board[i, j] == board.board[i, j + 3])
                    return board.board[i, j];
            }
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board.board[i, j] != 0 &&
                    board.board[i, j] == board.board[i, j + 1] &&
                    board.board[i, j] == board.board[i, j + 2] &&
                    board.board[i, j] == board.board[i, j + 3])
                    return board.board[i, j];
            }
        }

        //Check Vertical
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (board.board[i, j] != 0 &&
                    board.board[i, j] == board.board[i + 1, j] &&
                    board.board[i, j] == board.board[i + 2, j] &&
                    board.board[i, j] == board.board[i + 3, j])
                    return board.board[i, j];
            }
        }

        //Check Diagonal (top left to bottom right)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board.board[i, j] != 0 &&
                    board.board[i, j] == board.board[i + 1, j + 1] &&
                    board.board[i, j] == board.board[i + 2, j + 2] &&
                    board.board[i, j] == board.board[i + 3, j + 3])
                    return board.board[i, j];
            }
        }

        //Check Diagonal (top right to bottom left)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 3; j < 7; j++)
            {
                if (board.board[i, j] != 0 &&
                    board.board[i, j] == board.board[i + 1, j - 1] &&
                    board.board[i, j] == board.board[i + 2, j - 2] &&
                    board.board[i, j] == board.board[i + 3, j - 3])
                    return board.board[i, j];
            }
        }

        if (board.IsFull()) return 3;
        return 0;
    }

    public int NextWinningMove(Board board)
    {
        int returnValue = -1;
        int targetPlayer = board.currentPlayerTurn;

        //if the target can win in the next move, return that move
        //else if the opponent can win in the next move, block that move

        Parallel.For(1, 7, (i, state) =>
        {
            //Loop over both players
            for (int j = 1; j <= 2; j++)
            {
                Board tempBoard = new Board(board);
                if (tempBoard.MakeMove(i, j))
                {
                    if (WinningMove(tempBoard) == j)
                    {
                        returnValue = i;
                        state.Stop();
                    }
                }
            }
        });

        return returnValue;
    }
}