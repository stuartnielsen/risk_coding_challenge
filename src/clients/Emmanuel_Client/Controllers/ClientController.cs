using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Emmanuel_Client.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;

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
        public async Task<IActionResult> JoinServerAsync(string server)
        {
            serverAddress = server;
            var client = clientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "Emmanuel"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAddress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody]DeployArmyRequest deployArmyRequest)
        {
            return createDeployResponse(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return createAttackResponse(beginAttackRequest);
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

            //This logic will not grab a neighbour of the territory.
            foreach (var ter in beginAttack.Board) {
                if (!(ter.Owner.Name is null) && ter.Owner.Name == "Emmanuel")
                {
                    from = ter.Location;
                }
                if (!(ter.Owner.Name is null) && ter.Owner.Name != "Emmanuel")
                {
                    to = ter.Location;
                }

                if(!(from is null && to is null))
                {
                    break;
                }
            }

            return new BeginAttackResponse { From = from, To = to };
        }

        private DeployArmyResponse createDeployResponse(DeployArmyRequest deployArmyRequest)
        {
            Random r = new Random();
            int rInt;
            Location location = new Location();
            foreach(var ter in deployArmyRequest.Board)
            {
                rInt = r.Next(0, 3);
                if ((ter.Owner is null || ter.Owner.Name == "Emmanuel" ) && ter.Armies < rInt)
                {
                    location = ter.Location;
                    break;
                }
                else
                {
                    continue;
                }
            }

            return new DeployArmyResponse { DesiredLocation = location };
        }
    }
}
