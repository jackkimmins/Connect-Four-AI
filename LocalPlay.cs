using System;

namespace ConnectFourAI;

public class LocalPlay
{
    public void Play()
    {
        Board board = new Board();
        MiniMaxAlgorithm miniMax = new MiniMaxAlgorithm(12, true, true);

        int moveNum = 0;
        // int playersMove = -1;
        
        while (true)
        {
            board.PrintBoard(true);
            if (board.WinningMove(board) != 0)
            {
                cText.WriteLine("Player " + board.WinningMove(board) + " wins!", "CONNECT4", ConsoleColor.Green);
                break;
            }

            if (moveNum >= 20)
            {
                cText.WriteLine("Abort!", "CONNECT4", ConsoleColor.Red);
                break;
            }

            moveNum++;
            cText.WriteLine("Player " + board.currentPlayerTurn + " make a move: ", "CONNECT4", ConsoleColor.White);

            int move = -1;

            //Ensure that the move is valid an int between 1 and 7
            while (move < 1 || move > 7 || board.board[0, move - 1] != 0)
            {
                Console.Write("Enter column number (1-7): ");
                string input = Console.ReadLine() ?? "";

                if (input == "?")
                {
                    move = miniMax.GetBestMove(board).Column;
                    break;
                }
                else if (input == "q")
                {
                    Console.WriteLine("Quitting...");
                    return;
                }

                if (!int.TryParse(input, out move)) move = -1;
            }

            board.MakeMove(move);
        }

        Play();
    }
}