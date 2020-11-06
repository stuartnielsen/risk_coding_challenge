using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Game;
using Risk.Shared;

namespace Risk.Tests
{
    public class OwnershipTests
    {
        private Game.Game testgame;
        private string player1;
        private string player2;

        [SetUp]
        public void SetUp()
        {
            testgame = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            testgame.StartJoining();
            player1 = testgame.AddPlayer("player1");
            player2 = testgame.AddPlayer("player2");
            testgame.StartGame();

            //place 5 armies
            testgame.TryPlaceArmy(player1, new Location(0, 0));
            testgame.TryPlaceArmy(player1, new Location(0, 0));
            testgame.TryPlaceArmy(player1, new Location(0, 0));
            testgame.TryPlaceArmy(player1, new Location(0, 0));
            testgame.TryPlaceArmy(player1, new Location(0, 0));

            //player2 places 5 armies
            testgame.TryPlaceArmy(player2, new Location(0, 1));
            testgame.TryPlaceArmy(player2, new Location(0, 1));
            testgame.TryPlaceArmy(player2, new Location(0, 1));
            testgame.TryPlaceArmy(player2, new Location(0, 1));
            testgame.TryPlaceArmy(player2, new Location(0, 1));
        }

        //Player owns first territory, attacks second territory he doesn't own | should be true
        [Test]
        public void OwnershipValidityOwnerToForeign()
        {

            var placeResult = testgame.AttackOwnershipValid(player1, testgame.Board.GetTerritory(0, 0), testgame.Board.GetTerritory(0, 1));
            placeResult.Should().BeTrue();
        }

        //Player doesn't own first territory, attacks second territory he doesn't own | should be false
        [Test]
        public void OwnershipValidityForeignToForeign()
        {
            var placeResult = testgame.AttackOwnershipValid(player1, testgame.Board.GetTerritory(0, 1), testgame.Board.GetTerritory(0, 1));
            placeResult.Should().BeFalse();
        }

        //Player owns first territory, attacks second territory he also owns | should be false
        [Test]
        public void OwnershipValidityOwnerToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1, testgame.Board.GetTerritory(0, 0), testgame.Board.GetTerritory(0, 0));
            placeResult.Should().BeFalse();
        }
        //Player doesn't own first territory, attacks second territory he owns | should be false
        [Test]
        public void OwnershipValidityForeignToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1, testgame.Board.GetTerritory(0, 1), testgame.Board.GetTerritory(0, 0));
            placeResult.Should().BeFalse();
        }
    }
}
