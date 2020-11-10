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
    class RollDiceTest {

        private BeginAttackResponse newAttackResponse;
        private BeginAttackResponse newAttackResponse2;
        private BeginAttackResponse newAttackResponse3;
        private Game.Game game;

        [SetUp]
        public void Setup()
        {
            int width = 2;
            int height = 2;
            

            game = new Game.Game(new GameStartOptions { Height = height, Width = width });
            IPlayer player = new ApiPlayer("Rusty", "bazookaJoe", null);
            IPlayer player2 = new ApiPlayer("Emmanuel", "Macaco", null);
            Location attacker = new Location(1, 1);
            Location attacker2 = new Location(0, 1);
            Location defender = new Location(0, 0);
            Location defender2 = new Location(1, 0);
            newAttackResponse = new BeginAttackResponse {
                From = attacker,
                To= defender
            };
            newAttackResponse2 = new BeginAttackResponse {
                From = attacker2,
                To = defender2
            };
            newAttackResponse3 = new BeginAttackResponse {
                From = attacker,
                To = attacker2
            };
            game.TryPlaceArmy(player.Token, attacker);
            game.TryPlaceArmy(player.Token, attacker2);
            game.TryPlaceArmy(player2.Token, defender);
            game.TryPlaceArmy(player2.Token, defender2);    
        }
        [TestCase( newAttackResponse, ExpectedResult = false)]
        [TestCase(5, ExpectedResult = true)]
        [TestCase(-2, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = false)]
        public bool canRollDice(BeginAttackResponse beginAttackResponse)
        {
            game.RollDice(beginAttackResponse);
            
            return can;
        }
    }
}
//var deployArmyRequest = new DeployArmyRequest {
//    Board = game.Board.Territories,
//    Status = deploymentStatus,
//    ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
//};