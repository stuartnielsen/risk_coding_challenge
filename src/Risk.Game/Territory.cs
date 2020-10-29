using System;

namespace Risk.Game
{
    public class Territory
    {
        public Territory(Location location)
        {
            Location = location;
        }

        public Location Location { get; }

        public Player Owner { get; set; }
        public int Armies { get; set; }
    }
}
