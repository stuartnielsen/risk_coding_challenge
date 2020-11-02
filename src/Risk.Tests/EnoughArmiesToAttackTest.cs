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

        [SetUp]
        public void Setup()
        {
            int width = 1;
            int height = 1;

            game = new Game.Game(new GameStartOptions { Height = height, Width = width });
        }

        [TestCase(1,1,1)]
        [TestCase(1,1,5)]
        
        public void canAttackEnoughArmies(int attackerRow, int attackerColumn, int armies)
        {
            Location attacker = new Location(attackerRow,attackerColumn);
            Territory territory = new Territory(attacker);
            territory.Armies = armies;
            bool can = game.enoughArmiesToAttack(attacker);
            can.Should().BeTrue();
        }

        [TestCase(1, 1, -2)]
        [TestCase(1, 1, 0)]
        public void cannotAttackNotEnoughArmies(int attackerRow, int attackerColumn, int armies)
        {
            Location attacker = new Location(attackerRow, attackerColumn);
            Territory territory = new Territory(attacker);
            territory.Armies = armies;
            bool can = game.enoughArmiesToAttack(attacker);
            can.Should().BeFalse();
        }


    }
}
