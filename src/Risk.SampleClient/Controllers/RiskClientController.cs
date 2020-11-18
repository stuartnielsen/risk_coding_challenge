using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private static string serverAdress;

        public RiskClientController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server)
        {
            serverAdress = server;
            var client = httpClientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "braindead client"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAdress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch(Exception ex)
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

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody]DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            foreach (BoardTerritory space in deployArmyRequest.Board)
            {
                if ((space.OwnerName == null || space.OwnerName == "SampleClient1"))
                {
                    if (space.OwnerName == "SampleClient1" && space.Armies < 3)
                    {
                        response.DesiredLocation = space.Location;
                        continue;
                    }
                    else
                    {
                        response.DesiredLocation = space.Location;
                    }

                }
                return response;
            }
            return null;
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody]BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            var attackerLocation = new Location();
            //from is the attacker to is the defender
            foreach (BoardTerritory space in beginAttackRequest.Board)
            {
                if (space.OwnerName == "SampleClient1")
                {
                    attackerLocation = space.Location;
                    //look at the next location to the right, left, up, down, up-right diagonal, 
                    //down-right diagonal, up-left diagonal, down-left diagonal
                    for (int i = space.Location.Column - 1; i <= (space.Location.Column + 1); i++)
                    {
                        for (int j = space.Location.Row - 1; j <= (space.Location.Row + 1); j++)
                        {
                            if (space.OwnerName != "SampleClient1")
                            {
                                response.From = attackerLocation;
                                response.To = space.Location;
                                return response;
                            }
                        }
                    }

                }
            }
            return null;
        }

        [HttpPost("continueAttack")]
        public ContinueAttackResponse ContinueAttack([FromBody]ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = true;

            return response;
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody]GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }

    }
}
