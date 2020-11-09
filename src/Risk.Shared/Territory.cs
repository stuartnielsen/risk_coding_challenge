using System;

namespace Risk.Shared
{
    public class Territory
    {
        public Territory()
        {

        }

        public Territory(Location location)
        {
            Location = location;
        }

        public Location Location { get; set; }

        public Player Owner { get; set; }
        public int Armies { get; set; }

        public override string ToString()
        {
            if (Owner == null)
            {
                return $"{Location}: (Unoccupied)";
            }

            return $"{Location}: {Armies:n0} of {Owner}";
        }
    }
}
