using NUnit.Framework;
using Risk.Shared;
using Risk.Game;
using System.Collections.Generic;
using System.Linq;

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