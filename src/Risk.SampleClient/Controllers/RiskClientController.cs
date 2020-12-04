using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;
        private static string serverAdress;
        private GameStrat strat = new GameStrat();

        public RiskClientController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
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
            var myTerritory = deployArmyRequest.Board.FirstOrDefault(t => t.OwnerName == config["PlayerName"]) ??  deployArmyRequest.Board.Skip(deployArmyRequest.Board.Count() / 2).First(t => t.OwnerName == null);
            response.DesiredLocation = myTerritory.Location;
            return response;
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> board)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return board.Where(t => neighborLocations.Contains(t.Location));
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody]BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            foreach(var myTerritory in beginAttackRequest.Board.Where(t => t.OwnerName == config["PlayerName"]).OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, beginAttackRequest.Board);
                var destination = myNeighbors.Where(t => t.OwnerName != config["PlayerName"]).OrderBy(t => t.Armies).FirstOrDefault();
                if(destination != null)
                {
                    response.From = myTerritory.Location;
                    response.To = destination.Location;
                    return response;
                }
            }
            throw new Exception("No territory I can attack");
        }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody]ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = true;

            return response;
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
        public IActionResult GameOver([FromBody]GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }

    }
}
