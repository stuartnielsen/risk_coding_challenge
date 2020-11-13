using System.Collections.Generic;

namespace Risk.Shared
{
    public class DeployArmyRequest
    {
        public IEnumerable<BoardTerritory> Board { get; set; }
        public DeploymentStatus Status { get; set; }
        public int ArmiesRemaining { get; set; }
    }
}
