using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Risk.Game;

namespace Risk.Tests
{
    public class ArmyPlacementTests
    {
        Game.Game game;
        string player1;
        string player2;

        [SetUp]
        public void SetUp()
        {
            game = new Game.Game(new Game.GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5 });
            player1 = game.AddPlayer("player1");
            player2 = game.AddPlayer("player2");
        }

        [Test]
        public void CanPlaceArmyInUnoccupiedTerritory()
        {
            var placeResult = game.TryPlaceArmy(player1, new Location(0, 0));
            placeResult.Should().BeTrue();
        }

        [Test]
        public void CannotPlaceArmyInTerritoryOccupiedByAnotherPlayer()
        {
            game.TryPlaceArmy(player1, new Location(0, 0));
            var placeResult = game.TryPlaceArmy(player2, new Location(0, 0));
            placeResult.Should().BeFalse();
        }

        [Test]
        public void PlacingAnotherArmyOnTheSameTerritoryIncreasesTheNumberOfArmies()
        {
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            var territory = game.Board.GetTerritory(location);
            territory.Armies.Should().Be(2);
        }

        [Test]
        public void AfterPlacingTwoArmiesYouHaveThreeArmiesRemaining()
        {
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            throw new NotImplementedException();
        }
    }
}
