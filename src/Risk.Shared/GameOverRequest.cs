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
        public IEnumerable<BoardTerritory> FinalBoard { get; set; }
        /// <summary>
        /// Player name, final score
        /// </summary>
        public IEnumerable<string> FinalScores { get; set; }
        public string GameDuration { get; set; }
    }

    public class BoardTerritory
    {
        public Location Location { get; set; }
        public string OwnerName { get; set; }
        public int Armies { get; set; }
        public override string ToString() => $"{Location}: {Armies:n0} of {OwnerName ?? "(Unoccupied)"}";
    }
}
