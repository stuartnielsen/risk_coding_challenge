using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Risk.Shared;

namespace DJClient.Controllers
{
    
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpGet("AreYouThere")] 
        public string AreYouThere( )
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody]DeployArmyRequest deployArmyRequest)
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
    }
}
