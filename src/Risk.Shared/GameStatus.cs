using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<string> Players { get; }
        public IEnumerable<PlayerStats> PlayerStats { get; }
        public GameState GameState { get; set; }
        public IEnumerable<BoardTerritory> Board { get; set; }

        public GameStatus() { }

        public GameStatus(IEnumerable<string> players, GameState gameState, IEnumerable<BoardTerritory> board, IEnumerable<PlayerStats> playerStats )
        {
            Players = players;
            GameState = gameState;
            Board = board;
            PlayerStats = playerStats;
        }
    }

    public class PlayerStats
    {
        public string Name { get; set; }
        public int Armies { get; set; }
        public int Territories { get; set; }
    }
}
