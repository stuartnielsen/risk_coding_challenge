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
            List<BoardTerritory> myTerritories = new List<BoardTerritory>();
            foreach (var territory in deployRequest.Board)
            {
                if (territory.Armies == 0)
                {
                    deployResponse.DesiredLocation = territory.Location;
                    return deployResponse;
                }
                if (!(territory.OwnerName == null))
                {
                    if (territory.OwnerName == "Stuart")
                    {
                        myTerritories.Add(territory);
                        deployResponse.DesiredLocation = territory.Location;
                    }

                }
            }
            int min = myTerritories.First().Armies;
            foreach (var territory in myTerritories)
            {
                if (territory.Armies < min)
                    min = territory.Armies;
            }
            foreach (var territory in myTerritories)
            {
                if (territory.Armies == min)
                {
                    deployResponse.DesiredLocation = territory.Location;
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
                    if (territory.OwnerName == "Stuart")
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
                    if (territory.OwnerName == "Stuart" && territory.Armies == max)
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
                    if (neighbor.OwnerName != "Stuart" && neighbor.Armies < max)
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

        public ContinueAttackResponse DecideToContinueAttack(ContinueAttackRequest continueAttack)
        {
            ContinueAttackResponse attackResponse = new ContinueAttackResponse();
            attackResponse.ContinueAttacking = (continueAttack.AttackingTerritorry.Armies > continueAttack.DefendingTerritorry.Armies);
            return attackResponse;

        }
    }
}

