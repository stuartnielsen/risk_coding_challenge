using System;
using System.Collections.Generic;
using System.Linq;

namespace Risk.Game
{
    public class Game
    {
        public Game(GameStartOptions startOptions)
        {
            players = new List<Player>();
            Board = new Board(createTerritories(startOptions.Height, startOptions.Width));
            StartingArmies = startOptions.StartingArmiesPerPlayer;
        }

        private readonly List<Player> players;

        public Board Board { get; private set; }
        public int StartingArmies { get; }
        public IEnumerable<Player> Players => players.AsReadOnly();

        private IEnumerable<Territory> createTerritories(int height, int width)
        {
            var territories = new List<Territory>();
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    territories.Add(new Territory(new Location(r, c)));
                }
            }
            return territories;
        }

        public string AddPlayer(string playerName)
        {
            var p = new Player(name: playerName, token: Guid.NewGuid().ToString());
            players.Add(p);
            return p.Token;
        }

        public string Join(string playerName)
        {
            string gameState = "joining";
            if(gameState == "joining")
            {
                string playerToken = AddPlayer(playerName);
                return playerToken;
            }
            else
            {
                return "Can no longer join the game";
            }          
        }

        public bool TryPlaceArmy(string playerToken, Location desiredLocation)
        {
            var territory = Board.GetTerritory(desiredLocation);
            if (territory.Owner == null)
            {
                territory.Owner = getPlayer(playerToken);
                territory.Armies = 1;
                return true;
            }
            if (territory.Owner.Token != playerToken)
            {
                return false;
            }
            else //owner token == playerToken
            {
                if (GetPlayerRemainingArmies(playerToken) > 0)
                {
                    territory.Armies++;
                    return true;
                }
                return false;
            }
        }

        public int GetPlayerRemainingArmies(string playerToken)
        {
            var player = getPlayer(playerToken);
            var armiesOnBoard = Board.Territiories
                .Where(t => t.Owner == player)
                .Sum(t => t.Armies);
            return StartingArmies - armiesOnBoard;
        }

        private Player getPlayer(string token)
        {
            return players.Single(p => p.Token == token);
        }
    }
}
