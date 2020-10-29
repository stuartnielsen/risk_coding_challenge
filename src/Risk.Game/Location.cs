using System.Collections;

namespace Risk.Game
{
    public class Location
    {
        public Location(int row, int col)
        {
            Row = row;
            Column = col;
        }

        /// <summary>
        /// Starting from the bottom, row 0 is the bottom row
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Starting from the left.  Column 0 is the far left column.
        /// </summary>
        public int Column { get; }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                Location l => l.Row == Row && l.Column == Column,
                _ => base.Equals(obj)
            };
        }
    }
}