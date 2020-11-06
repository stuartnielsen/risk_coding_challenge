using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    /// <summary>
    /// The client sends this to the server telling the server who should attack who
    /// </summary>
    public class BeginAttackResponse
    {
        public Territory From { get; set; }
        public Territory To { get; set; }
    }
}
