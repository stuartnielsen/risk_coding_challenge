using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace StuartClient
{
    public class GameStrategy
    {
        public DeployArmyResponse DecideArmyWhereToPlacement(DeployArmyRequest deployRequest)
        {
            throw new NotImplementedException();
        }

        public BeginAttackResponse DecideWhereToAttack(BeginAttackRequest attackRequest)
        {
            throw new NotImplementedException();

        }

        public ContinueAttackResponse DecideToContinueAttack(ContinueAttackRequest continueAttack)
        { 
            throw new NotImplementedException();

        }
    }
}
