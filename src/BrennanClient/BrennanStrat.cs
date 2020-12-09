using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace BrennanClient
{
    public class BrennanStrat
    {
        public DeployArmyResponse DecideArmyWhereToPlacement(DeployArmyRequest deployRequest)
        {
            DeployArmyResponse deployResponse = new DeployArmyResponse();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(deployRequest.Board);
            int maxRow = 0;
            int maxCollumn = 0; 
            foreach(BoardTerritory bt in deployRequest.Board)
            {
                if(bt.Location.Row > maxRow)
                {
                    maxRow = bt.Location.Row;
                }
                if (bt.Location.Column > maxCollumn)
                {
                    maxCollumn = bt.Location.Column;
                }
            }
            List<Location> cornerLocations = new List<Location>();
            cornerLocations.Add(new Location(0, 0));
            cornerLocations.Add(new Location(0, maxCollumn));
            cornerLocations.Add(new Location(maxRow, 0));
            cornerLocations.Add(new Location(maxRow, maxCollumn));

            IEnumerable<BoardTerritory> corners = deployRequest.Board.Where(t => cornerLocations.Contains(t.Location));
            corners = corners.Reverse();

            if(myTerritories.Count() == 0)
            {
                foreach(BoardTerritory t in corners)
                {
                    if(t.OwnerName == null)
                    {
                        deployResponse.DesiredLocation = t.Location;
                        return deployResponse;
                    }
                }
                foreach(BoardTerritory t in deployRequest.Board.Reverse())
                {
                    if (t.OwnerName == null)
                    {
                        deployResponse.DesiredLocation = t.Location;
                        return deployResponse;
                    }
                }
            }
            else
            {
                BoardTerritory startTerritory = myTerritories.Last();
                IEnumerable<BoardTerritory> neighborsOfCorner = GetNeighbors(startTerritory, deployRequest.Board);
                foreach (BoardTerritory t in neighborsOfCorner)
                {
                    if(t.OwnerName != null && !myTerritories.Contains(t))
                    {
                        deployResponse.DesiredLocation = startTerritory.Location;
                    }
                    if(t.OwnerName == null)
                    {
                        deployResponse.DesiredLocation = t.Location;
                        return deployResponse;
                    }
                }
                foreach (BoardTerritory t in neighborsOfCorner)
                {
                    IEnumerable<BoardTerritory> friendlyNeighborsOfCorner = neighborsOfCorner.Where(ter => myTerritories.Contains(ter));
                    BoardTerritory smallPup = new BoardTerritory();
                    smallPup.Armies = 9999;
                    foreach (BoardTerritory bt in friendlyNeighborsOfCorner)
                    {
                        if (bt.Armies < smallPup.Armies)
                        {
                            smallPup = bt;
                        }
                    }
                    deployResponse.DesiredLocation = smallPup.Location;
                    return deployResponse;
                    
                }
            }
            
            return deployResponse;
        }

        public BeginAttackResponse DecideWhereToAttack(BeginAttackRequest attackRequest)
        {
            BeginAttackResponse beginAttack = new BeginAttackResponse();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(attackRequest.Board);
            BoardTerritory topDog = new BoardTerritory();
            topDog.Armies = 0;
            
            foreach(BoardTerritory t in myTerritories)
            {
                if(t.Armies > topDog.Armies)
                {
                    if (GetNumBadTerritories(t, attackRequest.Board) > 0)
                    {
                        topDog = t;
                    }
                }
            }
            beginAttack.From = topDog.Location;

            IEnumerable<BoardTerritory> tDogNeighbors = GetNeighbors(topDog, attackRequest.Board);
            BoardTerritory smallPup = new BoardTerritory();
            smallPup.Armies = 99999;
            foreach(BoardTerritory t in tDogNeighbors)
            {
                if (!myTerritories.Contains(t) && t.Armies < smallPup.Armies)
                {
                    smallPup = t;
                }
            }
            beginAttack.To = smallPup.Location;

            return beginAttack;
        }

        internal ContinueAttackResponse DecideToMakeNewAttack(ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(continueAttackRequest.Board);
            BoardTerritory topDog = new BoardTerritory();
            topDog.Armies = 0;
            foreach(BoardTerritory territory in myTerritories)
            {
                if(territory.Armies > topDog.Armies)
                {
                    if(GetNumBadTerritories(territory, continueAttackRequest.Board) > 0)
                    {
                        topDog = territory;
                    }
                }
            }
            IEnumerable<BoardTerritory> tDogNeighbors = GetNeighbors(topDog, continueAttackRequest.Board);
            BoardTerritory smallPup = new BoardTerritory();
            smallPup.Armies = 99999;
            foreach (BoardTerritory t in tDogNeighbors)
            {
                if (!myTerritories.Contains(t) && t.Armies < smallPup.Armies)
                {
                    smallPup = t;
                }
            }

            if (topDog.Armies > smallPup.Armies) response.ContinueAttacking = true;
            else response.ContinueAttacking = false;
            return response;
        }

        internal ManeuverResponse DecideWhereToManeuver(ManeuverRequest maneuverRequest)
        {
            ManeuverResponse response = new ManeuverResponse();
            response.Decide = false;

            return response;
        }

        internal DeployArmyResponse DecideWhereToReinforce(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritories = GetMyTerritories(deployArmyRequest.Board);
            BoardTerritory smallPup = new BoardTerritory();
            smallPup.Armies = 99999;
            foreach (BoardTerritory territory in myTerritories)
            {
                if(territory.Armies < smallPup.Armies)
                {
                    smallPup = territory;
                }
            }
            response.DesiredLocation = smallPup.Location;
            return response;
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> territories)
        {
            var l = territory.Location;
            Location[] neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return territories.Where(t => neighborLocations.Contains(t.Location));
        }

        private IEnumerable<BoardTerritory> GetMyTerritories(IEnumerable<BoardTerritory> territories)
        {
            List<BoardTerritory> myTerritories = new List<BoardTerritory>();
            foreach(BoardTerritory t in territories)
            {
                if(t.OwnerName != null && t.OwnerName == "Brennan")
                {
                    myTerritories.Add(t);
                }
            }
            return myTerritories;
        }

        public ContinueAttackResponse DecideToContinueAttack(ContinueAttackRequest continueAttack)
        {
            ContinueAttackResponse attackResponse = new ContinueAttackResponse();
            attackResponse.ContinueAttacking = (continueAttack.AttackingTerritorry.Armies > continueAttack.DefendingTerritorry.Armies);
            return attackResponse;

        }

        public int GetNumBadTerritories(BoardTerritory territory, IEnumerable<BoardTerritory> board)
        {
            int numBadTerritories = 0;
            IEnumerable<BoardTerritory> neigbors = GetNeighbors(territory, board);
            foreach(BoardTerritory possibleBadGuy in neigbors)
            {
                if(possibleBadGuy.OwnerName != "Brennan")
                {
                    numBadTerritories++;
                }
            }
            return numBadTerritories;
        }
    }
}

