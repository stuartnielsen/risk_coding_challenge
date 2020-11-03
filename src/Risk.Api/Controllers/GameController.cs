using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    public class GameController : Controller
    {
        private readonly Game.Game game;

        public GameController(Game.Game game)
        {
            this.game = game;
        }

        public IActionResult Index()
        {
            return View();
        }

        public string Join(string playerName, Uri callback)
        {
            if (game.GameState == GameState.Joining)
            {
                string playerToken = game.AddPlayer(playerName);
                return playerToken;
            }
            else
            {
                return "Can no longer join the game";
            }
        }
        public Game.Game InitializeGame (int height, int width, int numOfArmies, string secretCode)
        {
            GameStartOptions startOptions = new GameStartOptions();
            startOptions.GameState = GameState.Initializing;

            startOptions.Height = height;
            startOptions.Width = width;
            startOptions.StartingArmiesPerPlayer = numOfArmies;
            Game.Game newGame = new Game.Game(startOptions);

            game.StartJoining();
            startOptions.GameState = GameState.Joining;
           
            return newGame;
        }
    }
}
