using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameOverRequest
    {
        /// <summary>
        /// Name of the player with the highest score
        /// </summary>
        public string WinnerName { get; set; }
        /// <summary>
        /// Final game state
        /// </summary>
        public IEnumerable<Territory> FinalBoard { get; set; }
        /// <summary>
        /// Player name, final score
        /// </summary>
        public Dictionary<string, int> FinalScores { get; set; }
        public TimeSpan GameDuration { get; set; }
    }
}
