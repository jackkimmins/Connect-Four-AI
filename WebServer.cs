using System;
using Jindium;
using Newtonsoft.Json;

namespace ConnectFourAI;

class Game
{
    public Board board = new Board();
    public MiniMaxAlgorithm miniMax = new MiniMaxAlgorithm(17, true, true);
}

public class ConnectFourWebServer
{
    private Dictionary<string, Game> games = new Dictionary<string, Game>();

    private string CreateNewGame()
    {
        string gameId = Guid.NewGuid().ToString();
        games.Add(gameId, new Game());
        cText.WriteLine("New Game Started! ID: " + gameId);
        return gameId;
    }

    private void EndGame(string gameId, string reason)
    {
        games.Remove(gameId);
        cText.WriteLine("Game Ended! ID: " + gameId + " Reason: " + reason);
    }

    public void Start()
    {
        JinServer server = new JinServer();

        server.ServerRoutes.AddContentRoute("/", "/");

        server.ServerRoutes.AddStaticRoute("/newGame", Method.POST, async (ctx) =>
        {
            if (ctx.Session.ContainsKey("gameID"))
            {
                if (games.ContainsKey(ctx.Session["gameID"]))
                {
                    EndGame(ctx.Session["gameID"], "New Game Requested");
                }
            }

            string gameId = CreateNewGame();

            ctx.Session["gameID"] = gameId;

            await ctx.Send("done");
        });
        
        server.ServerRoutes.AddStaticRoute("/dropToken", Method.POST, async (ctx) => {
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
            
            var data = await ctx.GetRequestPostData();

            if (!data.ContainsKey("col"))
            {
                await ctx.Send("Bad Request, missing 'col' value.", 400);
                return;
            }

            if (!games[gameID].board.MakeMove(int.Parse(data["col"])))
            {
                await ctx.Send("Bad Request, invalid move.", 400);
                return;
            }

            if (games[gameID].board.WinningMove(games[gameID].board) != 0)
            {
                await ctx.Send("Game Over!", 203);
                EndGame(gameID, "Human Won");
                return;
            }

            //AI move
            // ReturnMove bestMove = games[gameID].miniMax.GetBestMove(games[gameID].board, int.Parse(data["col"]));
            ReturnMove bestMove = games[gameID].miniMax.GetBestMove(games[gameID].board);
            games[gameID].board.MakeMove(bestMove.Column);

            //print the board
            games[gameID].board.PrintBoard();

            int status = games[gameID].board.WinningMove(games[gameID].board) != 0 ? 200 : 201;

            //convert best move to json
            await ctx.Send(JsonConvert.SerializeObject(bestMove), status);

            if (games[gameID].board.WinningMove(games[gameID].board) != 0)
            {
                EndGame(gameID, "AI Won");
            }
        });

        server.Start();
    }
}