using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace Risk.SampleClient
{
    public class GameStrat
    {
        public ManeuverResponse DecideWhereToManeuver(ManeuverRequest maneuverRequest)
        {
            ManeuverResponse response = new ManeuverResponse();
            response.Decide = false;

            return response;
        }

        internal ContinueAttackResponse DecideToMakeNewAttack(ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = false;
            return response;
        }

        public DeployArmyResponse DecideWhereToReinforce(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritories = GetMyTerritories(deployArmyRequest.Board);
            BoardTerritory smallPup = new BoardTerritory();
            smallPup.Armies = 99999;
            foreach (BoardTerritory territory in myTerritories)
            {
                if (territory.Armies < smallPup.Armies)
                {
                    smallPup = territory;
                }
            }
            response.DesiredLocation = smallPup.Location;
            return response;
        }

        private IEnumerable<BoardTerritory> GetMyTerritories(IEnumerable<BoardTerritory> territories)
        {
            List<BoardTerritory> myTerritories = new List<BoardTerritory>();
            foreach (BoardTerritory t in territories)
            {
                if (t.OwnerName != null && t.OwnerName == "SampleClient")
                {
                    myTerritories.Add(t);
                }
            }
            return myTerritories;
        }
    }
}
