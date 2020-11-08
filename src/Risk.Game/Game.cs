using System;
using System.Collections.Generic;
using System.Linq;
using Risk.Shared;

namespace Risk.Game
{
    public class Game
    {
        public Game(GameStartOptions startOptions)
        {
            players = new List<Player>();
            Board = new Board(createTerritories(startOptions.Height, startOptions.Width));
            StartingArmies = startOptions.StartingArmiesPerPlayer;
            gameState = GameState.Initializing;
        }


        public Board Board { get; private set; }
        private GameState gameState { get; set; }
        public int StartingArmies { get; }

        public GameState GameState => gameState;


        private  List<Player> players;
        public IEnumerable<Player> Players => players;// { get; private set; }

        private IEnumerable<Territory> createTerritories(int height, int width)
        {
            var territories = new List<Territory>();
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    territories.Add(new Territory(new Location(r, c)));
                }
            }
            return territories;
        }

        public void StartJoining()
        {
            gameState = GameState.Joining;
        }

        public void StartGame()
        {
            gameState = GameState.Deploying;
        }

        public string AddPlayer(string playerName, string callbackAddress)
        {
            if (gameState == GameState.Joining)
            {
                var p = new Player(
                    name: playerName,
                    token: Guid.NewGuid().ToString(),
                    callbackAddress: callbackAddress);
                players.Add(p);
                return p.Token;
            }
            throw new InvalidGameStateException("Unable to join game.", gameState);
        }

        public bool TryPlaceArmy(string playerToken, Location desiredLocation)
        {
            var placeResult = false;

            if (gameState != Shared.GameState.Deploying)
                return false;

            if (GetPlayerRemainingArmies(playerToken) < 1)
                return false;

            var territory = Board.GetTerritory(desiredLocation);

            if (territory.Owner == null)
            {
                territory.Owner = getPlayer(playerToken);
                territory.Armies = 1;
                placeResult = true;
            }
            else if (territory.Owner.Token != playerToken)
            {
                placeResult = false;
            }
            else //owner token == playerToken
            {
                if (GetPlayerRemainingArmies(playerToken) > 0)
                {
                    territory.Armies++;
                    placeResult = true;
                }
                else
                {
                    placeResult = false;
                }
            }

            if (placeResult && CanChangeToAttackState())
                gameState = GameState.Attacking;

            return placeResult;
        }


        public int GetPlayerRemainingArmies(string playerToken)
        {
            var player = getPlayer(playerToken);
            var armiesOnBoard = getNumPlacedArmies(player);
            return StartingArmies - armiesOnBoard;
        }

        public Player getPlayer(string token)
        {
            return players.Single(p => p.Token == token);
        }

        public bool CanChangeToAttackState()
        {
            int totalRemainingArmies = 0;
            foreach(var p in players)
            {
                totalRemainingArmies += GetPlayerRemainingArmies(p.Token);
            }

            return (totalRemainingArmies == 0);
        }

        public bool EnoughArmiesToAttack(Territory attacker)
        {
            return attacker.Armies > 1;
        }

        public bool AttackOwnershipValid(string playerToken, Location from, Location to)
        {
            var territoryFrom = Board.Territories.Single(t => t.Location == from);
            var territoryTo = Board.Territories.Single(t => t.Location == to);
            var player = getPlayer(playerToken);
            return (territoryFrom.Owner == player && territoryTo.Owner != player);
        }


        public GameStatus GetGameStatus()
        {
            IDictionary<string, PlayerArmiesAndTerritories> playerInfo = new Dictionary<string, PlayerArmiesAndTerritories>();

            foreach(var player in Players)
            {
                int numPlacedArmies = getNumPlacedArmies(player);
                int numOwnedTerritories = Board.Territories.Where(t => t.Owner == player)
                                                            .Count();

                var armiesAndTerritories = new PlayerArmiesAndTerritories { NumArmies = numPlacedArmies, NumTerritories = numPlacedArmies };

                playerInfo.Add(player.Name, armiesAndTerritories);
            }

            return new GameStatus(players, GameState, playerInfo);
        }

        public int getNumPlacedArmies(Player player)
        {
            return Board.Territories
                        .Where(t => t.Owner == player)
                        .Sum(t => t.Armies);
        }

        public void RemovePlayer(string playerToken)
        {
            players.Remove(getPlayer(playerToken));
        }
    }
}