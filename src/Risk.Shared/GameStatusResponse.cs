using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatusResponse
    {
        public IEnumerable<Territory> Board { get; set; }
        public Dictionary<string, int> CurrentScores { get; set; }
        public TimeSpan GameDuration { get; set; }
        public DateTime StatusTimestamp { get; set; }
    }
}
