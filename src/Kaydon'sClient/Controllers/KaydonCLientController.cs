using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Kaydon_sClient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KaydonCLientController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private static string serverAdress;
        private int attackTimes = 0;
        private int ownedX = 3;
        private int ownedY = 2;

        public KaydonCLientController(IHttpClientFactory httpClientFactory)
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
            response.DesiredLocation = new Location(3, 2);
            return response;
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            response.From = new Location(ownedX, ownedY);
            if (attackTimes%5 == 0)
            {
                response.To = new Location(ownedX, ownedY--);
                attackTimes++;
            }
            else if (attackTimes % 5 == 1)
            {
                response.To = new Location(ownedX--, ownedY--);
                attackTimes++;
            }
            else if (attackTimes % 5 == 2)
            {
                response.To = new Location(ownedX--, ownedY);
                attackTimes++;
            }
            else if (attackTimes % 5 == 3)
            {
                response.To = new Location(ownedX--, ownedY++);
                attackTimes++;
            }
            else
            {
                response.To = new Location(ownedX, ownedY++);
                attackTimes++;
            }
            

            return response;
        }

        [HttpPost("continueAttack")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = false;

            return response;
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
