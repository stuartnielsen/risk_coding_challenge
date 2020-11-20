using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;



namespace WyattClient
{
    public class GameStrategy
    {
        public DeployArmyResponse WhereToPlace(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse deployArmyResponse = new DeployArmyResponse();
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName == null)
                {
                    deployArmyResponse.DesiredLocation = territory.Location;
                    return deployArmyResponse;
                }
            }
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName != null)
                {
                    if (territory.OwnerName == "Wyatt" && territory.Armies < 3)
                    {
                        deployArmyResponse.DesiredLocation = territory.Location;
                        return deployArmyResponse;
                    }
                }

            }
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName != null)
                {
                    if (territory.OwnerName == "Wyatt" && territory.Armies == 3)
                    {
                        deployArmyResponse.DesiredLocation = territory.Location;
                        return deployArmyResponse;
                    }
                }



            }
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName != null)
                {
                    if (territory.OwnerName == "Wyatt" && territory.Armies == 4)
                    {
                        deployArmyResponse.DesiredLocation = territory.Location;
                        return deployArmyResponse;
                    }
                }



            }
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName != null)
                {
                    if (territory.OwnerName == "Wyatt" && territory.Armies < 10)
                    {
                        deployArmyResponse.DesiredLocation = territory.Location;
                        return deployArmyResponse;
                    }
                }



            }
            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName != null)
                {
                    if (territory.OwnerName == "Wyatt")
                    {
                        deployArmyResponse.DesiredLocation = territory.Location;
                        return deployArmyResponse;
                    }
                }



            }
            return deployArmyResponse;
        }

        public BeginAttackResponse WhenToAttack(BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse beginAttack = new BeginAttackResponse();
            int myArmy = 0;
            IEnumerable<BoardTerritory> neighbors = new List<BoardTerritory>();
            foreach (var territory in beginAttackRequest.Board)
            {
                if (!(territory.OwnerName == null))
                {
                    if (territory.OwnerName == "Wyatt")
                    {
                        if (territory.Armies > myArmy)
                            myArmy = territory.Armies;
                    }

                }
            }
            foreach (var territory in beginAttackRequest.Board)
            {
                if (!(territory.OwnerName == null))
                {
                    if (territory.OwnerName == "Wyatt" && territory.Armies == myArmy)
                    {
                        beginAttack.From = territory.Location;
                        neighbors = GetNeighbors(territory, beginAttackRequest.Board);
                    }
                }
            }

            foreach (var neighbor in neighbors)
            {
                if (!(neighbor.OwnerName == null))
                {
                    if (neighbor.OwnerName != "Wyatt")
                        beginAttack.To = neighbor.Location;
                    if (neighbor.OwnerName != "Wyatt" && neighbor.Armies == 0)
                    {
                        beginAttack.To = neighbor.Location;
                        return beginAttack;
                    }
                    else if (neighbor.OwnerName != "Wyatt" && neighbor.Armies < myArmy)
                        beginAttack.To = neighbor.Location;
                }
            }
            return beginAttack;
        }



        public ContinueAttackResponse WhenToContinueAttack(ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse continueAttackResponse = new ContinueAttackResponse();
            if (continueAttackRequest.AttackingTerritorry.Armies > continueAttackRequest.DefendingTerritorry.Armies + 1)
            {
                continueAttackResponse.ContinueAttacking = true;
                return continueAttackResponse;
            }
            continueAttackResponse.ContinueAttacking = false;
            return continueAttackResponse;
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
    }
}