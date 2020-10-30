using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Risk.Tests
{
    public class PlayerTests
    {
        [Test]
        public void MakeAPlayer()
        {
            var game = new Game.Game(new Game.GameStartOptions { Height = 2, Width = 2 });
            var playerToken = game.AddPlayer("Player 1");
            Guid.TryParse(playerToken, out var _).Should().BeTrue();
            game.Players.Count().Should().Be(1);
        }
    }
}

