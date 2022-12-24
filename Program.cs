using System;

namespace ConnectFourAI;

class cText
{
    //Output the text to the console.
    public static void WriteLine(string value, string threadName = "CONNECT4", ConsoleColor consoleColor = ConsoleColor.DarkCyan)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{DateTime.Now.ToString("HH:mm:ss")}] [Thread/{threadName}]: ");
        Console.ResetColor();

        Console.ForegroundColor = consoleColor;
        Console.WriteLine(value);
        Console.ResetColor();
    }
}

public class Program
{
    static bool inputGiven = false;

    private static void AutoStartWebServer()
    {
        //Wait 5s
        Thread.Sleep(5000);

        if (!inputGiven)
        {
            cText.WriteLine("Starting web server automatically...", "CONNECT4", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConnectFourWebServer webServer = new ConnectFourWebServer();
            webServer.Start();
        }
    }

    public static void Main(string[] args)
    {
        //Ask the user if they would like a web server or a local game.
        cText.WriteLine("Would you like to play a local game or host a web server? (local/web)", "MAIN");

        cText.WriteLine("The web server will automatically start in 5s if no input is given.", "MAIN", ConsoleColor.DarkYellow);
        Task.Run(() => AutoStartWebServer());
        
        //Get the user's input
        string input = Console.ReadLine() ?? "";
        input = input.ToLower();
        inputGiven = true;

        do
        {
            if (input == "local")
            {
                LocalPlay localPlay = new LocalPlay();
                localPlay.Play();
                break;
            }
            else if (input == "web" || input == "")
            {
                ConnectFourWebServer webServer = new ConnectFourWebServer();
                webServer.Start();
                break;
            }
            else
            {
                //Ask the user to enter a valid input.
                cText.WriteLine("Please enter a valid input.", "MAIN", ConsoleColor.DarkRed);
                input = Console.ReadLine() ?? "";
            }
        } while (true);
    }
}