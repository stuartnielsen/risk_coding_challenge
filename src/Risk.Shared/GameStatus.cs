using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatus
    {
        public GameState GameState { get; set; }

        public IDictionary<string, PlayerArmiesAndTerritories> PlayerInfo { get; set; }

        public GameStatus() { }

        public GameStatus(GameState gameState, IDictionary<string, PlayerArmiesAndTerritories> playerInfo )
        {
            GameState = gameState;
            PlayerInfo = playerInfo;
        }
    }
}
