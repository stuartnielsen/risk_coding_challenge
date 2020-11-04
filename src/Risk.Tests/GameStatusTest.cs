using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Risk.Shared;

namespace Risk.Tests
{
    public class GameStatusTest
    {
        private Game.Game game;
        private Territory territory;
        [SetUp]
        public void Setup()
        {
            int width = 2;
            int height = 2;

            game = new Game.Game(new GameStartOptions { Height = height, Width = width, StartingArmiesPerPlayer = 5 });
        }
     
        [Test]
        public void GetGameStatusReturnsAllPlayersWhoveJoined()
        {
            var player1 = new Player("player1", "token");

            game.AddPlayer(player1.Name);

            var gameStatus = game.GetGameStatus();


            Assert.IsTrue(gameStatus.Players.Count<Player>()  == 1);
        }
     
        [Test]
        public void GetGameStatusHasGameState()
        {
            var gameStatus = game.GetGameStatus();

            Assert.That(gameStatus.GameState == GameState.Joining);
        }

        [Test]
        public void GetGameStatusHasPlayersWithArmyAndTerritoryCount()
        {
            string playerName = "Player1";
            game.AddPlayer(playerName);

            var player1 = game.Players.First();

            game.StartGame();

            game.TryPlaceArmy(player1.Token, new Location(0, 0));

            var gameStatus = game.GetGameStatus();


            Assert.That(gameStatus.PlayerInfo[player1.Name].NumTerritories == 1 && gameStatus.PlayerInfo[player1.Name].NumArmies == 1);
            
        }

    }
}
