using NUnit.Framework;
using Risk.Shared;
using Risk.Game;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace DJClient.Test
{
    public class GamePlayerTests
    {
        private GamePlayer gamePlayer;
        private IList<Territory> Board;
        private int playerArmyCount;


        private const int BOARD_ROWS = 5;
        private const int BOARD_COLS = 5;

        [SetUp]
        public void Setup()
        {
            gamePlayer = new GamePlayer { Player = new ClientPlayer { Name = "Player" , Token = "PlayerToken"} };
            playerArmyCount = 5;

            Board = new List<Territory>();
        }

        [Test]
        public void DeploysArmyOnVacantSpaceOnFirstTurn()
        {
            //Fill board except for one space
            fillBoard();
            Board.Remove(Board.Last());
            Board.Add(new Territory { 
                Armies = 0, 
                Location = new Location { Row = BOARD_ROWS - 1, Column = BOARD_COLS - 1}, 
                Owner = null 
            });


            var deployRequest = new DeployArmyRequest { Board = Board, ArmiesRemaining = playerArmyCount, Status = DeploymentStatus.YourTurn };

            var deployResponse = gamePlayer.DeployArmy(deployRequest);
            Location placementLocation = deployResponse.DesiredLocation;

            Assert.That(placementLocation.Row == (BOARD_ROWS - 1) && placementLocation.Column == (BOARD_COLS - 1));

        }

        [Test]
        public void DeploysArmyOnOwnedTerritoryIfNoVacantTerritories()
        {
            fillBoard();
            Board.Remove(Board.Last());
            Board.Add(new Territory {
                Armies = 1,
                Location = new Location { Row = BOARD_ROWS - 1, Column = BOARD_COLS - 1 },
                Owner = gamePlayer.Player
            });

            var deployRequest = new DeployArmyRequest { Board = Board, ArmiesRemaining = playerArmyCount, Status = DeploymentStatus.YourTurn };

            var deployResponse = gamePlayer.DeployArmy(deployRequest);
            Location placementLocation = deployResponse.DesiredLocation;

            Assert.That(placementLocation.Row == (BOARD_ROWS - 1) && placementLocation.Column == (BOARD_COLS - 1));
        }


        [Test]
        [Ignore("Can't figure out where previous attack came from when continueAttackRequest is just the board")]
        public void DoesNotAttackIfAttackSourceHasLessThanTwoArmies()
        {
            var attackSource = new Location(0, 0);
            var attackTarget = new Location(0, 1);

            Board.Add(new Territory { Armies = 1, Location = attackSource, Owner = gamePlayer.Player });
            Board.Add(new Territory { Armies = 1, Location = attackTarget, Owner = new ClientPlayer { Name = "Enemy" } });

            var continueAttackRequest  = new ContinueAttackRequest { Board = Board};
            var continueAttackResponse = gamePlayer.DecideContinueAttackResponse(continueAttackRequest);

            continueAttackResponse.ContinueAttacking.Should().Be(false);

        }


        [Test]
        public void AttacksIfAttackSourceHasTwoArmiesOrMore()
        {
            var attackSource = new Location(0, 0);
            var attackTarget = new Location(0, 1);

            Board.Add(new Territory { Armies = 2, Location = attackSource, Owner = gamePlayer.Player });
            Board.Add(new Territory { Armies = 1, Location = attackTarget, Owner = new ClientPlayer { Name = "Enemy" } });

            var continueAttackRequest = new ContinueAttackRequest { Board = Board };
            var continueAttackResponse = gamePlayer.DecideContinueAttackResponse(continueAttackRequest);

            continueAttackResponse.ContinueAttacking.Should().Be(true);
        }


        [Test]
        public void areAdjacentReturnsTrueIfArmiesAreAdjacent()
        {
            Territory territory1 = new Territory { Location = new Location(0, 0) };
            Territory territory2 = new Territory { Location = new Location(0, 1) };

            gamePlayer.areAdjacent(territory1, territory2).Should().BeTrue();

        }

        [Test]
        public void areAdjacentReturnsFalseIfArmiesAreNotAdjacent()
        {
            Territory territory1 = new Territory { Location = new Location(0, 0) };
            Territory territory2 = new Territory { Location = new Location(0, 2) };

            gamePlayer.areAdjacent(territory1, territory2).Should().BeFalse();
        }

        [Test]
        public void DecideBeginAttackAttacksFirstNeighbor()
        {
            var attackSource = new Territory { Armies = 1, Owner = gamePlayer.Player, Location = new Location(0, 0) };
            var attackTarget = new Territory { Armies = 1, Owner = new ClientPlayer { Name = "Enemy"}, Location = new Location(0, 1) };

            Board.Add(attackSource);
            Board.Add(attackTarget);

            var beginAttackRequest = new BeginAttackRequest { Board = Board, Status = BeginAttackStatus.YourTurn };
            var beginAttackResponse = gamePlayer.DecideBeginAttack(beginAttackRequest);

            Assert.That(beginAttackResponse.From == attackSource.Location && beginAttackResponse.To == attackTarget.Location);
        }


        private void fillBoard()
        {
            for (int rowIndex = 0; rowIndex < BOARD_ROWS; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < BOARD_COLS; ++colIndex)
                {
                    Board.Add(new Territory {
                        Armies = 1,
                        Owner = new ClientPlayer { Name = "Opponent", Token = "" },
                        Location = new Location(rowIndex, colIndex)
                    });

                }
            }
        }
    }
}