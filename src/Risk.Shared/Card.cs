using System;
using System.Collections.Generic;
using System.Text;

namespace Risk.Shared
{
    public class Card
    {
        public string Type { get; set; }
        public Location location{ get; set; }

        public Card()
        {
            var random = new Random();
            var cardTypes = new List<string> { "Infantry", "Cavalry", "Artillery"};
            Type = cardTypes[random.Next(cardTypes.Count)];
        }
    }
}
