using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Risk.Shared;

namespace Risk.Api.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private Game.Game game;
        private IMemoryCache memoryCache;
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration config;
        private readonly List<ApiPlayer> players;
        private readonly ConcurrentBag<ApiPlayer> joiningPlayers;
        private readonly ILogger<GameRunner> logger;
        private readonly List<ApiPlayer> removedPlayers = new List<ApiPlayer>();

        public GameController(Game.Game game, IMemoryCache memoryCache, IHttpClientFactory client, IConfiguration config, List<ApiPlayer> players, ConcurrentBag<ApiPlayer> joiningPlayers, ILogger<GameRunner> logger)
        {
            this.game = game;
            this.clientFactory = client;
            this.config = config;
            this.players = players;
            this.joiningPlayers = joiningPlayers;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Join(JoinRequest joinRequest)
        {
            if (game.GameState == GameState.Joining && await ClientIsRepsonsive(joinRequest.CallbackBaseAddress))
            {
                var newPlayer = new ApiPlayer(
                    name: joinRequest.Name,
                    token: Guid.NewGuid().ToString(),
                    httpClient: clientFactory.CreateClient()
                );
                newPlayer.HttpClient.BaseAddress = new Uri(joinRequest.CallbackBaseAddress);

                joiningPlayers.Add(newPlayer);
                return Ok(new JoinResponse {
                    Token = newPlayer.Token
                });
            }
            else
            {
                return BadRequest("Unable to join game");
            }
        }

        private async Task<bool> ClientIsRepsonsive(string baseAddress)
        {
            //client.CreateClient().BaseAddress = new Uri(baseAddress);
            var response = await clientFactory.CreateClient().GetStringAsync($"{baseAddress}/areYouThere");
            return response.ToLower() == "yes";
        }

        [HttpGet("status")]
        public IActionResult GameStatus()
        {
            GameStatus gameStatus;
            players.Clear();
            players.AddRange(joiningPlayers);

            if (!memoryCache.TryGetValue("Status", out gameStatus))
            {
                gameStatus = game.GetGameStatus();

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();

                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);

                memoryCache.Set("Status", gameStatus, cacheEntryOptions);
            }

            return Ok(gameStatus);
        }

        public static Game.Game InitializeGame (int height, int width, int numOfArmies, IEnumerable<IPlayer> players)
        {
            GameStartOptions startOptions = new GameStartOptions {
                Height = height,
                Width = width,
                StartingArmiesPerPlayer = numOfArmies,
                Players = players
            };
            Game.Game newGame = new Game.Game(startOptions);

            newGame.StartJoining();
            return newGame;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> StartGame(StartGameRequest startGameRequest)
        {
            if(game.GameState != GameState.Joining)
            {
                return BadRequest("Game not in Joining state");
            }
            if(config["secretCode"] != startGameRequest.SecretCode)
            {
                return BadRequest("Secret code doesn't match, unable to start game.");
            }
            players.Clear();
            players.AddRange(joiningPlayers);
            game.StartGame();
            var gameRunner = new GameRunner(game, players, removedPlayers, logger);
            await gameRunner.StartGameAsync();
            return Ok();
        }
    }
}
