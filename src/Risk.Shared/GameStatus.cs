using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<IPlayer> Players { get; }
        public GameState GameState { get; set; }

        public IDictionary<string, PlayerArmiesAndTerritories> PlayerInfo { get; set; }

        public GameStatus() { }

        public GameStatus(IEnumerable<IPlayer> players, GameState gameState, IDictionary<string, PlayerArmiesAndTerritories> playerInfo )
        {
            Players = players;
            GameState = gameState;
            PlayerInfo = playerInfo;
        }
    }
}
