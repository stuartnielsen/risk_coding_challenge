using NUnit.Framework;
using Risk.Shared;
using Risk.Game;
using System.Collections.Generic;

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
            gamePlayer = new GamePlayer();
            playerArmyCount = 5;

            Board = new List<Territory>();
            for (int rowIndex = 0; rowIndex < BOARD_ROWS; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < BOARD_COLS; ++colIndex)
                {
                    Board.Add(new Territory { Armies = 0, Owner = null, Location = new Location(rowIndex, colIndex) });
                }
            }
            
        }

        [Test]
        public void DeploysArmyOnVacantSpaceOnFirstTurn()
        {
            //Fill board except for one space
            for (int rowIndex = 0; rowIndex < BOARD_ROWS; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < BOARD_COLS; ++colIndex)
                {
                    if (rowIndex != BOARD_ROWS && colIndex != BOARD_COLS)
                    {
                        Board.Add(new Territory {
                            Armies = 1,
                            Owner = new ClientPlayer { Name = "Opponent", Token = "" },
                            Location = new Location(rowIndex, colIndex)
                        });
                    }
                }
            }


            var deployRequest = new DeployArmyRequest { Board = Board, ArmiesRemaining = playerArmyCount, Status = DeploymentStatus.YourTurn };

            var deployResponse = gamePlayer.DeployArmy(deployRequest);
            Location placementLocation = deployResponse.DesiredLocation;

            Assert.That(placementLocation.Row == BOARD_ROWS && placementLocation.Column == BOARD_COLS);

        }

        
    }
}