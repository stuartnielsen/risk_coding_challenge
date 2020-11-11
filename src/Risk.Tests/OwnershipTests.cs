using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Api;
using Risk.Game;
using Risk.Shared;

namespace Risk.Tests
{
    public class OwnershipTests
    {
        private Game.Game testgame;
        private string player1Token;
        private string player2Token;
        private List<ApiPlayer> players;

        [SetUp]
        public void SetUp()
        {
            players = new List<ApiPlayer>();
            testgame = new Game.Game(new GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5, Players = players });
            testgame.StartJoining();
            player1Token= Guid.NewGuid().ToString();
            player2Token= Guid.NewGuid().ToString();
            players.Add(new ApiPlayer("player1", player1Token, null));
            players.Add(new ApiPlayer("player2", player2Token, null));

            testgame.StartGame();

            //place 5 armies
            testgame.TryPlaceArmy(player1Token, new Location(0, 0));
            testgame.TryPlaceArmy(player1Token, new Location(0, 0));
            testgame.TryPlaceArmy(player1Token, new Location(0, 0));
            testgame.TryPlaceArmy(player1Token, new Location(0, 0));
            testgame.TryPlaceArmy(player1Token, new Location(0, 0));

            //player2 places 5 armies
            testgame.TryPlaceArmy(player2Token, new Location(0, 1));
            testgame.TryPlaceArmy(player2Token, new Location(0, 1));
            testgame.TryPlaceArmy(player2Token, new Location(0, 1));
            testgame.TryPlaceArmy(player2Token, new Location(0, 1));
            testgame.TryPlaceArmy(player2Token, new Location(0, 1));
        }

        //Player owns first territory, attacks second territory he doesn't own | should be true
        [Test]
        public void OwnershipValidityOwnerToForeign()
        {

            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 0), new Location(0, 1));
            placeResult.Should().BeTrue();
        }

        //Player doesn't own first territory, attacks second territory he doesn't own | should be false
        [Test]
        public void OwnershipValidityForeignToForeign()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 1), new Location(0, 1));
            placeResult.Should().BeFalse();
        }

        //Player owns first territory, attacks second territory he also owns | should be false
        [Test]
        public void OwnershipValidityOwnerToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 0), new Location(0, 0));
            placeResult.Should().BeFalse();
        }
        //Player doesn't own first territory, attacks second territory he owns | should be false
        [Test]
        public void OwnershipValidityForeignToOwner()
        {
            var placeResult = testgame.AttackOwnershipValid(player1Token, new Location(0, 1), new Location(0, 0));
            placeResult.Should().BeFalse();
        }

        [Test]
        public void PlayerHasAtLeastOnePlaceToAttack()
        {
            var actual = testgame.PlayerCanAttack(players[1]);
            actual.Should().BeTrue();
        }
    }
}
