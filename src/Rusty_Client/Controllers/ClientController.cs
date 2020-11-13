using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Risk.Shared;

namespace Rusty_Client.Controllers
{
    
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;

        public RiskClientController(IHttpClientFactory clientFactory)
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
                Name = "Rusty"
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
            DeployArmyResponse response = new DeployArmyResponse();
            response.DesiredLocation = new Location(1, 1);
            return response;
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            response.From = new Location(1, 1);
            response.To = new Location(1, 2);
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
     

        private BeginAttackResponse createAttackResponse(BeginAttackRequest beginAttack)
        {
            var from = new Location();
            var to = new Location();
                foreach(var territory in beginAttack.Board)
            {
                if(!(territory.Owner.Name is null)&& territory.Owner.Name == "Rusty")
                {
                    from = territory.Location;
                }
                if(!(from is null && to is null))
                {
                    break;
                }
            }
            return new BeginAttackResponse { From = from, To = to };
        }
        private DeployArmyResponse createDeployRequest(DeployArmyRequest deployArmyRequest)
        {
            var location = new Location();
            foreach(var territory in deployArmyRequest.Board)
            {
                if(territory.Owner is null || territory.Owner.Name =="Rusty")
                {
                    location = territory.Location;
                }
            }
            return new DeployArmyResponse { DesiredLocation = location };
        }
    }
}
