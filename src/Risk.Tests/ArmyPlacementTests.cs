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
            game = new Game.Game(new Game.GameStartOptions { Height = 2, Width = 2, StartingArmiesPerPlayer = 5, GameState = "Deployment" });
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

            var remainingArmies = game.GetPlayerRemainingArmies(player1);
            remainingArmies.Should().Be(3);
        }

        [Test]
        public void CannotPlaceArmyIf0ArmiesRemaining()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            var placeResult = game.TryPlaceArmy(player1, location);
            var remainingArmies = game.GetPlayerRemainingArmies(player1);
            placeResult.Should().BeTrue();
            remainingArmies.Should().Be(0);

            //Act
            placeResult = game.TryPlaceArmy(player1, location);

            //Assert
            placeResult.Should().BeFalse();
        }
        [Test]
        public void GameStateChangesIfNobodyHasArmies()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            var location2 = new Location(0, 1);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);

            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);

            var placeResult1 = game.TryPlaceArmy(player1, location);
            var placeResult2 = game.TryPlaceArmy(player2, location2);

            var remainingArmies1 = game.GetPlayerRemainingArmies(player1);
            var remainingArmies2 = game.GetPlayerRemainingArmies(player2);

            placeResult1.Should().BeTrue();
            placeResult2.Should().BeTrue();
            remainingArmies1.Should().Be(0);
            remainingArmies2.Should().Be(0);

            //Act
            game.ChangeState();
            string gState = game.GetState();

            //Assert
            gState.Should().Be("Attack");
        }
        [Test]
        public void GameStateWillNotChangeIfPeopleHaveArmies()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            var location2 = new Location(0, 1);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);

            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);
            game.TryPlaceArmy(player2, location2);

            var placeResult1 = game.TryPlaceArmy(player1, location);
            var placeResult2 = game.TryPlaceArmy(player2, location2);

            var remainingArmies1 = game.GetPlayerRemainingArmies(player1);
            var remainingArmies2 = game.GetPlayerRemainingArmies(player2);

            placeResult1.Should().BeTrue();
            placeResult2.Should().BeTrue();
            remainingArmies1.Should().Be(2);
            remainingArmies2.Should().Be(0);

            //Act
            game.ChangeState();
            string gState = game.GetState();

            //Assert
            gState.Should().Be("Deployment");
        }
    }
}
