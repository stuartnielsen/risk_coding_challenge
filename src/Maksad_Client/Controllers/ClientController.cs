using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Maksad_Client.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        //private static string serverName = "http://localhost:5000";

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

        }


        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return createDeployResponse(deployArmyRequest);
        }

        private DeployArmyResponse createDeployResponse(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            foreach (BoardTerritory space in deployArmyRequest.Board)
            {
                if ((space.OwnerName == null || space.OwnerName == "Maksad"))
                {
                    if (space.OwnerName == "Maksad" && space.Armies < 3)
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
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return createAttackResponse(beginAttackRequest); ;
        }
        private BeginAttackResponse createAttackResponse(BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            var attackerLocation = new Location();
            //from is the attacker to is the defender
            foreach (BoardTerritory space in beginAttackRequest.Board)
            {
                if (space.OwnerName == "Maksad")
                {
                    attackerLocation = space.Location;
                    //look at the next location to the right, left, up, down, up-right diagonal, 
                    //down-right diagonal, up-left diagonal, down-left diagonal
                    for (int i = space.Location.Column - 1; i <= (space.Location.Column + 1); i++)
                    {
                        for (int j = space.Location.Row - 1; j <= (space.Location.Row + 1); j++)
                        {
                            if (space.OwnerName != "Maksad")
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
