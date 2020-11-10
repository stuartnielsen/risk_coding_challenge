using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<string> Players { get; }
        public GameState GameState { get; set; }
        public IEnumerable<BoardTerritory> Board { get; set; }

        public GameStatus() { }

        public GameStatus(IEnumerable<string> players, GameState gameState, IEnumerable<BoardTerritory> board )
        {
            Players = players;
            GameState = gameState;
            Board = board;
        }
    }
}
