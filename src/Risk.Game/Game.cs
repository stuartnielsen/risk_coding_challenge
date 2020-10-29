using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Game
{
    public class Game
    {
        public Game(int height, int width)
        {
            players = new List<Player>();
            Board = new Board(createTerritories(height, width));
        }

        private IEnumerable<Territory> createTerritories(int height, int width)
        {
            var territories = new List<Territory>();
            for(int r = 0; r < height; r++)
            {
                for(int c = 0; c < width; c++)
                {
                    territories.Add(new Territory(new Location(r, c)));
                }
            }
            return territories;
        }

        private readonly List<Player> players;
        public IEnumerable<Player> Players => players.AsReadOnly();

        public Board Board { get; private set; }
    }
}
