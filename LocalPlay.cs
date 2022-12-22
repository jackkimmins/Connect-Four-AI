using System;

namespace ConnectFourAI;

public class LocalPlay
{
    public void Play()
    {
        Board board = new Board();
        MiniMaxAlgorithm miniMax = new MiniMaxAlgorithm(11, true);

        int moveNum = 0;
        
        while (true)
        {
            board.PrintBoard(true);
            if (board.WinningMove(board) != 0)
            {
                Console.WriteLine("Player " + board.WinningMove(board) + " wins!");
                break;
            }

            if (moveNum >= 10)
            {
                Console.WriteLine("Abort!");
                break;
            }

            moveNum++;
        
            //Let the current player make a move
            Console.WriteLine("Player " + board.currentPlayerTurn + " make a move: ");
            
            if (board.currentPlayerTurn == 1)
            {
                // int move = -1;

                // //Ensure that the move is valid an int between 1 and 7
                // while (move < 1 || move > 7 || board.board[0, move - 1] != 0)
                // {
                //     Console.Write("Enter column number (1-7): ");
                //     string input = Console.ReadLine() ?? "";

                //     if (input == "?")
                //     {
                //         move = miniMax.GetBestMove(board).Column;
                //         break;
                //     }
                //     else if (input == "q")
                //     {
                //         Console.WriteLine("Quitting...");
                //         return;
                //     }

                //     if (!int.TryParse(input, out move)) move = -1;
                // }

                // board.MakeMove(move);

                Random rand = new Random();
                int move = rand.Next(1, 24);
                board.MakeMove(move < 8 ? move : miniMax.GetBestMove(board).Column);
            }
            else
            {
                ReturnMove bestMove = miniMax.GetBestMove(board);
                board.MakeMove(bestMove.Column);
                Console.WriteLine("Best move: " + bestMove.Column + " with score: " + bestMove.Score + " and iterations: " + bestMove.Iterations);
            }
        }

        Play();
    }
}