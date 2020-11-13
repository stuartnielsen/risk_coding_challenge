using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace TannerClient.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private static string serverAdress;

        int x = 0;
        int y = 0;
        int attempts = 0;

        public ClientController(IHttpClientFactory httpClientFactory)
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
                Name = "Tanner Client"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAdress}/join", joinRequest);
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

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            if (deployArmyRequest.Status == DeploymentStatus.PreviousAttemptFailed)
            {
                if(x < 5)
                {
                    response.DesiredLocation = new Location(x++, y);
                    return response;
                }
                else if(y < 5)
                {
                    response.DesiredLocation = new Location(x, y++);
                    return response;
                }
                else
                {
                    x = 0;
                    y = 0;
                }
            }

            response.DesiredLocation = new Location(x, y);
            return response;
           
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            
            if (beginAttackRequest.Status == BeginAttackStatus.PreviousAttackRequestFailed)
            {
                attempts++;
                if (attempts == 1 && y < 5)
                {
                    response.From = new Location(x, y);
                    response.To = new Location(x, y+1);
                    return response;
                }
                else if (attempts == 2 && x < 5)
                {
                    response.From = new Location(x, y);
                    response.To = new Location(x + 1, y);
                    return response;
                }
                else if (attempts == 3 && x > 0)
                {
                    response.From = new Location(x, y);
                    response.To = new Location(x - 1, y);
                    return response;
                }
                else if (attempts == 4 && x < 5 && y < 5)
                {
                    response.From = new Location(x, y);
                    response.To = new Location(x + 1, y + 1);
                    return response;
                }
                else if (attempts == 5 && x > 0 && y > 0)
                {
                    response.From = new Location(x, y);
                    response.To = new Location(x - 1, y - 1);
                    return response;
                }
            }

            attempts = 0;
            response.From = new Location(x, y);
            response.To = new Location(x, y - 1);
            return response;
        }

        [HttpPost("continueAttack")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = true;

            return response;
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
