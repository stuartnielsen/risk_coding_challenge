using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Shared;

namespace Risk.Tests
{
    public class PlayerTests
    {
        [Test]
        public void MakeAPlayer()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2 });
            game.StartJoining();
            var playerToken = game.AddPlayer("Player 1", "");
            Guid.TryParse(playerToken, out var _).Should().BeTrue();
            game.Players.Count().Should().Be(1);
        }

        [Test]
        public void AddPlayerAfterGameStarts()
        {
            var game = new Game.Game(new GameStartOptions { Height = 2, Width = 2 });//THis was Game.GameStartOptions. Was it intended to be??
            game.StartJoining();
            var playerToken = game.AddPlayer("Player 1", "");
            Guid.TryParse(playerToken, out var _).Should().BeTrue();
            game.StartGame();
            Assert.Throws<InvalidGameStateException>(() => game.AddPlayer("Player 2", ""));
        }
    }
}

