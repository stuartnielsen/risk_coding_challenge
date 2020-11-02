using System.Collections.Generic;

namespace Risk.Shared
{
    public class StatusResponse
    {
        public IEnumerable<string> Players { get; set; }
        public string GameState { get; set; }
        public IEnumerable<Territory> Board { get; set; }
    }
}
