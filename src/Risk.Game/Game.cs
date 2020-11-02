using System;
using System.Collections.Generic;
using System.Linq;
using Risk.Shared;

namespace Risk.Game
{
    public class Game
    {
        public Game(GameStartOptions startOptions)
        {
            players = new List<Player>();
            Board = new Board(CreateTerritories(startOptions.Height, startOptions.Width));
            StartingArmies = startOptions.StartingArmiesPerPlayer;
            gameState = GameState.Joining;
        }

        private readonly List<Player> players;

        public Board Board { get; private set; }
        private GameState gameState { get; set; }
        public int StartingArmies { get; }

        public GameState GameState => gameState;

        public IEnumerable<Player> Players => players.AsReadOnly();

        private IEnumerable<Territory> CreateTerritories(int height, int width)
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

        public void StartGame()
        {
            gameState = GameState.Deploying;
        }

        public string AddPlayer(string playerName)
        {
            if (gameState == GameState.Joining)
            {
                var p = new Player(name: playerName, token: Guid.NewGuid().ToString());
                players.Add(p);
                return p.Token;
            }
            return "game already started";
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
