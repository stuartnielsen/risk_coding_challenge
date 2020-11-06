using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Risk.Api;
using Risk.Shared;

namespace Risk.Tests
{
    public class GameRunnerTests
    {
        private Game.Game game;
        private string player1;
        private string player2;
        private GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 1 });
            game.StartJoining();
            player1 = game.AddPlayer("player1");
            player2 = game.AddPlayer("player2");
            game.StartGame();
            gameRunner = new GameRunner(game, null);
        }

        [Test]
        public void IsAllArmiesPlacedReturnsTrueIfAllArmiesArePlace()
        {
            game.TryPlaceArmy(player1, new Location(0, 0));
            game.TryPlaceArmy(player2, new Location(1, 0));

            Assert.IsTrue(gameRunner.IsAllArmiesPlaced());
        }

        [Test]
        public void IsAllArmiesPlacedReturnsFalseIfOnePlayerHasRemainingArmies()
        {
            game.TryPlaceArmy(player1, new Location(0, 0));
            Assert.IsFalse(gameRunner.IsAllArmiesPlaced());
        }
        

        [Test]
        public void IsAllArmiesPlacedReturnsFalseIfPlayersHaveRemainingArmies()
        {
            Assert.IsFalse(gameRunner.IsAllArmiesPlaced());
        }

    }
}
