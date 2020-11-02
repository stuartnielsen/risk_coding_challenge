using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Game;

namespace Risk.Tests
{
    class EnoughArmiesToAttackTest
    {
        private Game.Game game;
        private Territory territory;
        [SetUp]
        public void Setup()
        {
            int width = 2;
            int height = 2;

            game = new Game.Game(new GameStartOptions { Height = height, Width = width });
            Player player = new Player("Rusty", "kc7wzl");
            Location attacker = new Location(1,1);
            territory = new Territory(attacker);
            territory.Owner = player;
           

        }

        [TestCase(1, ExpectedResult =false)]
        [TestCase(5, ExpectedResult = true)]
        [TestCase(-2, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = false)]
        
        public bool canAttackEnoughArmies(int armies)
        {
            territory.Armies = armies;
            bool can = game.enoughArmiesToAttack(territory);
      
            return can;
        }

     


    }
}
