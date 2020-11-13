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

        public ContinueAttackResponse DecideContinueAttackResponse(ContinueAttackRequest continueAttackRequest)
        {
            throw new NotImplementedException();
        }

    }
}
