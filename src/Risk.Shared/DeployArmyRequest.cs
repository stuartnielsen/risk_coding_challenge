using System.Collections.Generic;

namespace Risk.Shared
{
    public class DeployArmyRequest
    {
        public IEnumerable<Territory> Board { get; set; }
        public DeploymentStatus Status { get; set; }
        public int ArmiesRemaining { get; set; }
    }
}
