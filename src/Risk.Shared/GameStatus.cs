using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<Player> Players {  get; }
        public GameState GameState { get; }

        public IDictionary<string, PlayerArmiesAndTerritories> PlayerInfo { get; }

        public GameStatus(IEnumerable<Player> players, GameState gameState, IDictionary<string, PlayerArmiesAndTerritories> playerInfo )
        {
            Players = players;
            GameState = gameState;
            PlayerInfo = playerInfo;
        }
    }
}
