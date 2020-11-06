using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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

        //player tokens (needed to send request to every player)
 

        [HttpPost("[action]")]
        public async Task<IActionResult> Join(JoinRequest joinRequest)
        {
            var response = await CheckClientConnection(joinRequest.CallbackBaseAddress);
            if (game.GameState == GameState.Joining && response == "yes")
            {
                string playerToken = game.AddPlayer(joinRequest.Name, joinRequest.CallbackBaseAddress);
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

        // send request to player and wait for response 
        [HttpGet ("placeArmy/{*deployArmyRequest}")]
        public async Task<IActionResult> PlaceArmyRequest ()
        {
            Player player = game.Players.First();
            DeployArmyRequest deployRequest = new DeployArmyRequest();
            deployRequest.Board = game.Board.Territiories;
            deployRequest.ArmiesRemaining = game.GetPlayerRemainingArmies(player.Token);
            deployRequest.Status = player.deploymentStatus;

            var localclient = client.CreateClient();
           

            try
            {
                var deployArmyResponse = await localclient.PostAsJsonAsync($"{player.CallbackBaseAddress}/deployArmy", deployRequest);
                var content = await deployArmyResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        //react to second response
        [HttpGet ("[action]")]
        public async Task<IActionResult> PlaceArmy_Get (DeployArmyResponse response)
        {
            game.TryPlaceArmy(game.Players.First().Token, response.DesiredLocation);
            return RedirectToRoute("/status");
        }

        [HttpPost ("placeArmy")]
        public async Task<IActionResult> PlaceArmy_Post ()
        {
            
            await PlaceArmyRequest( );
            //temporary doesnt make sense.
            return RedirectToRoute("/status");
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
