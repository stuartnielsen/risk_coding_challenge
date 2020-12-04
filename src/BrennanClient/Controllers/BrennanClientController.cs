using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace BrennanClient.Controllers
{
    public class BrennanClientController : Controller
    {

        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;
        private BrennanStrat strat = new BrennanStrat();

        public BrennanClientController(IHttpClientFactory clientFactory)
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
                Name = "Brennan"
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
            return strat.DecideArmyWhereToPlacement(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return strat.DecideWhereToAttack(beginAttackRequest);
        }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return strat.DecideToContinueAttack(continueAttackRequest);
        }

        [HttpPost("reinforce")]
        public DeployArmyResponse Reinforce([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return strat.DecideWhereToReinforce(deployArmyRequest);
        }

        [HttpPost("manuever")]
        public ManeuverResponse Maneuver([FromBody] ManeuverRequest maneuverRequest)
        {
            return strat.DecideWhereToManeuver(maneuverRequest);
        }

        [HttpPost("makeNewAttack")]
        public ContinueAttackResponse makeNewAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return strat.DecideToMakeNewAttack(continueAttackRequest);
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
