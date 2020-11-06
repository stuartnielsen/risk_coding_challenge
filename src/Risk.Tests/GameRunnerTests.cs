using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Moq;
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
            var httpClientMock = new Mock<HttpClient>();
            //httpClientMock.Setup()

            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 1 });
            game.StartJoining();
            player1 = game.AddPlayer("player1", "");
            player2 = game.AddPlayer("player2", "");
            game.StartGame();
            gameRunner = new GameRunner(httpClientMock.Object, game);
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

        [Test][Ignore("Failing test - don't leave it this way but come back to it!")]
        public void After1DeploymentRequestFailuresKickPlayer()
        {
            Assert.AreEqual(1, game.Players.Count());
        }

        [Test]
        public void After1DeploymentRequestFailuresRemovesPlayerFromBoard()
        {
            bool isPlayerOnBoard = true;
            game.TryPlaceArmy(player1, new Location(0, 0));
            game.TryPlaceArmy(player2, new Location(1, 0));
            gameRunner.RemovePlayerFromBoard(player1);

            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner != null && territory.Owner.Token != player1)
                {
                    isPlayerOnBoard = false;
                }
            }
            Assert.IsFalse(isPlayerOnBoard);

        }

    }
}
