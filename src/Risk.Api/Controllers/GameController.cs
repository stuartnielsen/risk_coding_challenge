using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

            string gameState = "joining";
            if (gameState == "joining")
            {
                string playerToken = game.AddPlayer(playerName);
                return playerToken;
            }
            else
            {
                return "Can no longer join the game";
            }
        }
    }
}
