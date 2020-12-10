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
            IEnumerable<BoardTerritory> neighbors = new List<BoardTerritory>();
            IEnumerable<BoardTerritory> myTerritories = GetMyTerritories(beginAttackRequest.Board);

            do
            {


                foreach (var territory in myTerritories)
                {
                    var myNeighbors = GetNeighbors(territory, beginAttackRequest.Board);

                    foreach (var neighbor in myNeighbors)
                    {

                        if ((territory.Armies > 1000 || territory.Armies > neighbor.Armies * 2) && neighbor.OwnerName != "Wyatt" && territory.Armies > 1)
                        {
                            beginAttack.From = territory.Location;
                            beginAttack.To = neighbor.Location;
                            return beginAttack;
                        }
                        if (neighbor.Armies < 2 && territory.Armies > 3 && neighbor.OwnerName != "Wyatt")
                        {
                            beginAttack.To = neighbor.Location;
                            beginAttack.From = territory.Location;
                            return beginAttack;
                        }
                        if (neighbor.OwnerName != "Wyatt" && territory.Armies > 1)
                        {
                            beginAttack.To = neighbor.Location;
                            beginAttack.From = territory.Location;
                        }
                    }
                }
            } while (beginAttack.To == null || beginAttack.From == null);

            return beginAttack;
        }



        public ContinueAttackResponse WhenToContinueAttack(ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse attackResponse = new ContinueAttackResponse();
            attackResponse.ContinueAttacking = (continueAttackRequest.AttackingTerritorry.Armies > continueAttackRequest.DefendingTerritorry.Armies);
            return attackResponse;
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

        public ManeuverResponse DecideWhereToManeuver(ManeuverRequest maneuverRequest)
        {
            ManeuverResponse response = new ManeuverResponse();
            response.Decide = false;

            return response;
        }

        internal ContinueAttackResponse DecideToMakeNewAttack(ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = false;
            return response;
        }

        public DeployArmyResponse DecideWhereToReinforce(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritories = GetMyTerritories(deployArmyRequest.Board);
            BoardTerritory smallPup = new BoardTerritory();
            smallPup.Armies = 99999;
            foreach (BoardTerritory territory in myTerritories)
            {
                if (territory.Armies < smallPup.Armies)
                {
                    smallPup = territory;
                }
            }
            response.DesiredLocation = smallPup.Location;
            return response;
        }

        private IEnumerable<BoardTerritory> GetMyTerritories(IEnumerable<BoardTerritory> territories)
        {
            List<BoardTerritory> myTerritories = new List<BoardTerritory>();
            foreach (BoardTerritory t in territories)
            {
                if (t.OwnerName != null && t.OwnerName == "Wyatt")
                {
                    myTerritories.Add(t);
                }
            }
            return myTerritories;
        }
    }
}