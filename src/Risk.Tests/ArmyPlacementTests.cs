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
        public void EndingArmyDeploymentState_StartingArmyAttackingState()
        {
            //AAA: arrange, act, assert
            //Arrange
            var location = new Location(0, 0);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);
            game.TryPlaceArmy(player1, location);               //Placing 5th army

            var remainingArmies = game.GetPlayerRemainingArmies(player1);

            remainingArmies.Should().Be(0);       //remainingArmies should be 0

            var armyDeploymentState = game.ArmyDeploymentState;

            armyDeploymentState.Should().BeFalse();      //If remainingArmies is 0, then armyDeploymentState is automatically false

            var placeResult = game.TryPlaceArmy(player1, location);      //Trying to place army after armyDeploymentState is False
            placeResult.Should().BeFalse();           //Result returns false

            var armyAttackingState = game.ArmyAttackingState;    //If armyDeploymentState is false, then ArmyAttackingState should be true
            armyAttackingState.Should().BeTrue();
        }
    }
}
