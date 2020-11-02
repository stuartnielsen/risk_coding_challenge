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
            player1 = game.AddPlayer("player1");
            player2 = game.AddPlayer("player2");
        }

        [Test]
        public void CanChangeStateFromJoiningToArmyDeployment()
        {
            var actual = game.ChangeStateFromJoiningToDeployments();
            var expected = Game.Game.GameStates.deployment;

            Assert.AreEqual(expected, actual);
                
        }

        [Test]
        public void CanOnlyChangeStateToDeployingIfStateIsJoining()
        {
            var gamestateStart = game.GetGameState();

        }
    }
}
