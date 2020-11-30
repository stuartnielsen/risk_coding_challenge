using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Risk.Shared;
using WyattClient;

namespace WyattClient.Controllers
{

    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;
        private GameStrategy gameStrategy = new GameStrategy();


        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }



        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }



        [HttpGet("/joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server)
        {
            serverAddress = server;
            var client = clientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {



                CallbackBaseAddress = baseUrl,
                Name = "Wyatt"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAddress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }



        [HttpPost("joinServer")]
        public async Task<IActionResult> JoinAsync_Post(string server)
        {
            await JoinAsync(server);
            return RedirectToPage("/GameStatus", new { servername = server });
        }


        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return gameStrategy.WhereToPlace(deployArmyRequest);
        }


        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return gameStrategy.WhenToAttack(beginAttackRequest);
        }


        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return gameStrategy.WhenToContinueAttack(continueAttackRequest);
        }


        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
