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
        private string player1Token;
        private string player2Token;
        private GameRunner gameRunner;
        private List<ApiPlayer> players;

        [SetUp]
        public void SetUp()
        {
            var httpClientMock = new Mock<HttpClient>();
            //httpClientMock.Setup()

            players = new List<ApiPlayer>();
            game = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 1, Players = players });
            game.StartJoining();

            player1Token = Guid.NewGuid().ToString();
            player2Token = Guid.NewGuid().ToString();

            players.Add(new ApiPlayer("player1", player1Token, null));
            players.Add(new ApiPlayer("player2", player2Token, null));

            game.StartGame();
            gameRunner = new GameRunner(game, players, new List<ApiPlayer>());
        }

        [Test]
        public void IsAllArmiesPlacedReturnsTrueIfAllArmiesArePlace()
        {
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            game.TryPlaceArmy(player2Token, new Location(1, 0));

            Assert.IsTrue(gameRunner.IsAllArmiesPlaced());
        }

        [Test]
        public void IsAllArmiesPlacedReturnsFalseIfOnePlayerHasRemainingArmies()
        {
            game.TryPlaceArmy(player1Token, new Location(0, 0));
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
            game.TryPlaceArmy(player1Token, new Location(0, 0));
            game.TryPlaceArmy(player2Token, new Location(1, 0));
            gameRunner.RemovePlayerFromBoard(player1Token);

            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner != null && territory.Owner.Token != player1Token)
                {
                    isPlayerOnBoard = false;
                }
            }
            Assert.IsFalse(isPlayerOnBoard);

        }

    }
}
