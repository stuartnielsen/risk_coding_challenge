using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Risk.Shared;

namespace Risk.Game
{
    public class Board
    {
        public Board(IEnumerable<Territory> territiories)
        {
            Territiories = territiories;
        }

        public IEnumerable<Territory> Territiories { get; }

        public Territory GetTerritory(int row, int col)
        {
            return Territiories.Single(t => t.Location.Row == row && t.Location.Column == col);
        }

        public Territory GetTerritory(Location location)
        {
            return Territiories.Single(t => t.Location == location);
        }

        public IEnumerable<Territory> GetNeighbors(Territory territory)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return Territiories.Where(t => neighborLocations.Contains(t.Location));
        }

        public bool AttackTargetLocationIsValid(Location attackSource, Location attackTarget)
        {
            int rowDistance = Math.Abs(attackSource.Row - attackTarget.Row);
            int colDistance = Math.Abs(attackSource.Column - attackTarget.Column);

            return (colDistance <= 1 && rowDistance <= 1) && (colDistance == 1 || rowDistance == 1);
        }
    }
}
