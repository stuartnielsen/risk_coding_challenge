using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Shared;
using Risk.Game;

namespace Risk.Tests
{
    public class StartGameTests
    {

        Game.Game game;
        string player1;
        string player2;

        [SetUp]
        public void SetUp()
        {
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            game.StartJoining();
            player1 = game.AddPlayer("player1");
            player2 = game.AddPlayer("player2");
        }

        [Test]
        public void CanChangeStateFromJoiningToArmyDeployment()
        {
            Assert.AreEqual(GameState.Joining, game.GameState);

            game.StartGame();

            var actual = game.GameState;
            Assert.AreEqual(GameState.Deploying, actual);
        }

    }
}
