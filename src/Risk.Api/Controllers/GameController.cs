using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : Controller
    {
        private readonly Game.Game game;
        private readonly HttpClient client;

        public GameController(Game.Game game)
        {
            this.game = game;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("[action]")]
        public string Join(string playerName, Uri callback)
        {
            if (game.GameState == GameState.Joining )
            {
                string playerToken = game.AddPlayer(playerName);
                return playerToken;
            }
            else
            {
                return "Can no longer join the game";
            }
        }

        public async Task<string> CheckClientConnection(Uri uri)
        {
            var response = await client.GetAsync(uri.AbsoluteUri);
            return response.Content.ToString();
        }
    }
}
