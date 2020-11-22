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
            cornerLocations.Add(new Location(maxCollumn, 0));
            cornerLocations.Add(new Location(0, maxRow));
            cornerLocations.Add(new Location(maxCollumn, maxRow));

            IEnumerable<BoardTerritory> corners = deployRequest.Board.Where(t => cornerLocations.Contains(t.Location));
            foreach(BoardTerritory t in corners)
            {
                if (myTerritories.Contains(t))
                {
                    deployResponse.DesiredLocation = t.Location;
                    return deployResponse;
                } 
                else if(t.OwnerName == null)
                {
                    deployResponse.DesiredLocation = t.Location;
                    return deployResponse;
                }
            }
            foreach(BoardTerritory t in deployRequest.Board)
            {
                if (myTerritories.Contains(t))
                {
                    deployResponse.DesiredLocation = t.Location;
                    return deployResponse;
                }
                else if (t.OwnerName == null)
                {
                    deployResponse.DesiredLocation = t.Location;
                    return deployResponse;
                }
            }
            return deployResponse;
        }

        public BeginAttackResponse DecideWhereToAttack(BeginAttackRequest attackRequest)
        {
            BeginAttackResponse beginAttack = new BeginAttackResponse();
            int max = 0;
            IEnumerable<BoardTerritory> neighbors = new List<BoardTerritory>();
            foreach (var territory in attackRequest.Board)
            {
                if (!(territory.OwnerName == null))
                {
                    if (territory.OwnerName == "Brennan")
                    {
                        if (territory.Armies > max)
                            max = territory.Armies;
                    }
                }
            }
            foreach (var territory in attackRequest.Board)
            {
                if (!(territory.OwnerName == null))
                {
                    if (territory.OwnerName == "Brennan" && territory.Armies == max)
                    {
                        beginAttack.From = territory.Location;
                        neighbors = GetNeighbors(territory, attackRequest.Board);
                    }
                }
            }

            foreach (var neighbor in neighbors)
            {
                if (!(neighbor.OwnerName == null))
                {
                    if (neighbor.OwnerName != "Brennan")
                        beginAttack.To = neighbor.Location;
                    if (neighbor.OwnerName != "Brennan" && neighbor.Armies == 0)
                    {
                        beginAttack.To = neighbor.Location;
                        return beginAttack;
                    }
                    else if (neighbor.OwnerName != "Brennan" && neighbor.Armies < max)
                        beginAttack.To = neighbor.Location;
                }
            }
            return beginAttack;
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> territories)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
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
    }
}

