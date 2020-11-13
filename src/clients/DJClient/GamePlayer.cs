using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace DJClient
{
    public class GamePlayer
    {
        public IPlayer Player{ get; set; }



        public DeployArmyResponse DeployArmy(DeployArmyRequest deployArmyRequest)
        {
            

            foreach(Territory territory in deployArmyRequest.Board)
            {
                if (territory.Owner == null)
                {
                    return new DeployArmyResponse { DesiredLocation = territory.Location };
                }
            }

            foreach (Territory territory in deployArmyRequest.Board)
            {
                if (territory.Owner.Token == Player.Token)
                {
                    return new DeployArmyResponse { DesiredLocation = territory.Location };
                }
            }

            throw new Exception("Cannot place army");
        }

        public BeginAttackResponse DecideBeginAttack(BeginAttackRequest beginAttackRequest)
        {
            var ownedTerritories = beginAttackRequest.Board.Where(t => t.Owner == Player);
            var enemyTerritories = beginAttackRequest.Board.Where(t => t.Owner != null && t.Owner != Player);

            foreach(Territory ownedTerritory in ownedTerritories)
            {
                foreach(Territory enemyTerritory in enemyTerritories)
                {
                    if (areAdjacent(ownedTerritory, enemyTerritory))
                    {
                        return new BeginAttackResponse { From = ownedTerritory.Location, To = enemyTerritory.Location };
                    }
                }
            }

            throw new Exception("Cannot attack");

        }

        public ContinueAttackResponse DecideContinueAttackResponse(ContinueAttackRequest continueAttackRequest)
        {
            return new ContinueAttackResponse { ContinueAttacking = true };
        }

        public bool areAdjacent(Territory territory1, Territory territory2)
        {
            int rowDistance = Math.Abs(territory1.Location.Row - territory2.Location.Row);
            int colDistance = Math.Abs(territory1.Location.Column - territory2.Location.Column);

            return (colDistance <= 1 && rowDistance <= 1) && (colDistance == 1 || rowDistance == 1);
        }

    }
}
