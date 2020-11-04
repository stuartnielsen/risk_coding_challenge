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
        private readonly IHttpClientFactory client;

        public GameController(Game.Game game, IHttpClientFactory client)
        {
            this.game = game;
            this.client = client;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("[action]")]
        public async Task<string> Join(JoinRequest joinRequest)
        {
            var response = await CheckClientConnection(joinRequest.CallbackBaseAddress);
            if (game.GameState == GameState.Joining && response == "yes")
            {
                string playerToken = game.AddPlayer(joinRequest.Name);
                return playerToken;
            }
            else
            {
                return "Can no longer join the game";
            }
        }

        public async Task<string> CheckClientConnection(string baseAddress)
        {
            //client.CreateClient().BaseAddress = new Uri(baseAddress);
            var response = await client.CreateClient().GetStringAsync($"{baseAddress}/areYouThere");
            return response;
        }
    }
}
