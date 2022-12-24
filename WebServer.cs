using System;
using Jindium;
using Newtonsoft.Json;

namespace ConnectFourAI;

class Game
{
    public Board board = new Board();
    public MiniMaxAlgorithm miniMax = new MiniMaxAlgorithm(11, true, true);
}

public class ConnectFourWebServer
{
    //Stores the currently running games, both with their IDs and the game objects
    private Dictionary<string, Game> games = new Dictionary<string, Game>();

    //Creates a new game on the server
    private string CreateNewGame()
    {
        string gameId = Guid.NewGuid().ToString();
        games.Add(gameId, new Game());
        cText.WriteLine("New Game Started! ID: " + gameId);
        return gameId;
    }

    //Terminates a game on the server
    private void EndGame(string gameId, string reason)
    {
        games.Remove(gameId);
        cText.WriteLine("Game Ended! ID: " + gameId + " Reason: " + reason);
    }

    public void Start()
    {
        //The Jinium web server - using the config.json file to change the hosting address and port
        JinServer server = new JinServer();

        //Adds the routes for the JinSite
        server.ServerRoutes.AddContentRoute("/", "/");

        //New Game Route - will create a new game and set the gameID in the session
        server.ServerRoutes.AddStaticRoute("/newGame", Method.POST, async (ctx) =>
        {
            if (ctx.Session.ContainsKey("gameID"))
            {
                if (games.ContainsKey(ctx.Session["gameID"])) EndGame(ctx.Session["gameID"], "New Game Requested");
            }

            ctx.Session["gameID"] = CreateNewGame();
            await ctx.Send("done");
        });
        
        //Drop Token Route - Will drop a token in the column specified, calls the AI to make a move, and returns the board
        server.ServerRoutes.AddStaticRoute("/dropToken", Method.POST, async (ctx) => {
            //If a game is not in progress, return an error
            if (ctx.Session.ContainsKey("gameID"))
            {
                if (!games.ContainsKey(ctx.Session["gameID"]))
                {
                    await ctx.Send("No Game Found, please ensure that you have started one.", 406);
                    return;
                }
            }
            else
            {
                await ctx.Send("No Game Found, please ensure that you have started one.", 406);
                return;
            }

            string gameID = ctx.Session["gameID"];
            
            //Gets the POST data from the request
            var data = await ctx.GetRequestPostData();

            //Ensure that the user has sent a column number where they want to drop the token
            if (!data.ContainsKey("col"))
            {
                await ctx.Send("Bad Request, missing 'col' value.", 400);
                return;
            }

            //Ensure that the column number is valid
            if (!games[gameID].board.MakeMove(int.Parse(data["col"])))
            {
                await ctx.Send("Bad Request, invalid move.", 400);
                return;
            }

            //Check if that move won the game
            if (games[gameID].board.WinningMove(games[gameID].board) != 0)
            {
                await ctx.Send("Game Over!", 203);
                EndGame(gameID, "Human Won");
                return;
            }

            //Make the AI move
            ReturnMove bestMove = games[gameID].miniMax.GetBestMove(games[gameID].board);
            games[gameID].board.MakeMove(bestMove.Column);

            //Print the board to the console
            games[gameID].board.PrintBoard();

            //The status code is used to tell the client if the game is over or not
            int status = games[gameID].board.WinningMove(games[gameID].board) != 0 ? 200 : 201;

            //Convert best move to JSON and return it to the client
            await ctx.Send(JsonConvert.SerializeObject(bestMove), status);

            //If the AI won, end the game and purge it from the dictionary
            if (games[gameID].board.WinningMove(games[gameID].board) != 0) EndGame(gameID, "AI Won");
        });

        server.Start();
    }
}