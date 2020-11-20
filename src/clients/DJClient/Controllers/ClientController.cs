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
        private GamePlayer gamePlayer;

        private GameOverRequest gameOver;

        public ClientController(IHttpClientFactory clientFactory, IPlayer player)
        {
            this.clientFactory = clientFactory;

            gamePlayer = new GamePlayer { Player = player };

        }

        [HttpGet("AreYouThere")] 
        public string AreYouThere( )
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody]DeployArmyRequest deployArmyRequest)
        {
            return gamePlayer.DeployArmy(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return gamePlayer.DecideBeginAttack(beginAttackRequest);
        }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return gamePlayer.DecideContinueAttackResponse(continueAttackRequest);
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            gameOver = gameOverRequest;
            return Ok(gameOverRequest);
        }

        [HttpGet("winner")]
        public GameOverRequest Winner()
        {
            return gameOver;
        }
    }
}
