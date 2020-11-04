using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private readonly Game.Game game;
        private IMemoryCache memoryCache;
        private readonly IHttpClientFactory client;

        public GameController(Game.Game game, IMemoryCache memoryCache, IHttpClientFactory client)
        {
            this.game = game;
            this.client = client;
            this.memoryCache = memoryCache;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Join(JoinRequest joinRequest)
        {
            var response = await CheckClientConnection(joinRequest.CallbackBaseAddress);
            if (game.GameState == GameState.Joining && response == "yes")
            {
                string playerToken = game.AddPlayer(joinRequest.Name);
                return Ok(new JoinResponse {
                    Token = playerToken
                });
            }
            else
            {
                return BadRequest("Unable to join game");
            }
        }

        private async Task<string> CheckClientConnection(string baseAddress)
        {
            //client.CreateClient().BaseAddress = new Uri(baseAddress);
            var response = await client.CreateClient().GetStringAsync($"{baseAddress}/areYouThere");
            return response;
        }

        [HttpGet("status")]
        public IActionResult GameStatus()
        {
            GameStatus gameStatus;

            if (!memoryCache.TryGetValue("Status", out gameStatus))
            {
                gameStatus = game.GetGameStatus();

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();

                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);

                memoryCache.Set("Status", gameStatus, cacheEntryOptions);
            }

            return Ok(gameStatus);
        }

        public static Game.Game InitializeGame (int height, int width, int numOfArmies)
        {
            GameStartOptions startOptions = new GameStartOptions();
            startOptions.Height = height;
            startOptions.Width = width;
            startOptions.StartingArmiesPerPlayer = numOfArmies;
            Game.Game newGame = new Game.Game(startOptions);

            newGame.StartJoining();
            return newGame;
        }
    }
}
