﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace BrennanClient
{
    public class BrennanStrat
    {
        private static BoardTerritory startTerritory { get; set; }
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
                        startTerritory = t;
                        return deployResponse;
                    }
                }
                foreach(BoardTerritory t in deployRequest.Board.Reverse())
                {
                    if (t.OwnerName == null)
                    {
                        deployResponse.DesiredLocation = t.Location;
                        startTerritory = t;
                        return deployResponse;
                    }
                }
                
            }
            else
            {
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
            IEnumerable <BoardTerritory> board = attackRequest.Board.Reverse();
            BeginAttackResponse beginAttack = new BeginAttackResponse();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(attackRequest.Board).Reverse();
            BoardTerritory topDog = new BoardTerritory();
            topDog.Armies = 0;
            //int maxNumBadTerritories = 8;
            
            foreach(BoardTerritory t in myTerritories)
            {
                if(t.Armies > topDog.Armies)
                {
                    if (GetNumBadTerritories(t, board) > 0 )
                    {
                        topDog = t;
                        //maxNumBadTerritories = GetNumBadTerritories(t, board);
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

            if (topDog.Armies > (2 * smallPup.Armies)) response.ContinueAttacking = true;
            else response.ContinueAttacking = false;
            return response;
        }

        internal ManeuverResponse DecideWhereToManeuver(ManeuverRequest maneuverRequest)
        {
            ManeuverResponse response = new ManeuverResponse();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(maneuverRequest.Board);
            BoardTerritory fromTerritory = new BoardTerritory();
            fromTerritory.Armies = 0;
            foreach(BoardTerritory territory in myTerritories)
            {
                if(GetNumBadTerritories(territory, maneuverRequest.Board) == 0 && territory.Armies > fromTerritory.Armies)
                {
                    fromTerritory = territory;
                }
            }
            if (fromTerritory.Armies == 0)
            {
                response.Decide = false;
            }
            else
            {
                response.Decide = true;
                response.From = fromTerritory.Location;
                BoardTerritory toTerritory = new BoardTerritory();
                int toScore = 999999;
                foreach (BoardTerritory territory in GetNeighbors(fromTerritory, maneuverRequest.Board))
                {
                    if (territory.Location.Column + territory.Location.Row < toScore)
                    {
                        toScore = territory.Location.Column + territory.Location.Row;
                        toTerritory = territory;
                    }
                }
                response.To = toTerritory.Location;
            }

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
                if(territory.Armies < smallPup.Armies && GetNumBadTerritories(territory, deployArmyRequest.Board) > 0)
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

