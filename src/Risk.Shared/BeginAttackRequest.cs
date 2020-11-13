using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    /// <summary>
    /// The server sends this to the client, giving them the current board state and asking where they want to attack
    /// </summary>
    public class BeginAttackRequest
    {
        public IEnumerable<BoardTerritory> Board { get; set; }
        public BeginAttackStatus Status { get; set; }
    }

    public enum BeginAttackStatus
    {
        YourTurn,
        PreviousAttackRequestFailed
    }
}
