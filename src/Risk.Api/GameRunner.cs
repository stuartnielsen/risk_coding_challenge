using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly HttpClient client;
        private readonly Game.Game game;
        public const int MaxFailedTries = 5;
        public Dictionary<String, int> PlayerFailedConsecutiveAttempts;

        public GameRunner(HttpClient client, Game.Game game)
        {
            this.client = client;
            this.game = game;
        }

        public async Task StartGameAsync()
        {
            foreach(Player player in game.Players)
            {
                PlayerFailedConsecutiveAttempts.Add(player.Token, 0);
            }
            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
            
        }

        private async Task deployArmiesAsync()
        {
            while(game.Board.Territories.Sum(t=>t.Armies) < game.StartingArmies * game.Players.Count())
            {
                foreach (var currentPlayer in game.Players)
                {
                    var deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.YourTurn);

                    var failedTries = 0;
                    //check that this location exists and is available to be used (e.g. not occupied by another army)
                    while (game.TryPlaceArmy(currentPlayer.Token, deployArmyResponse.DesiredLocation) is false)
                    {
                        failedTries++;
                        if (failedTries == MaxFailedTries)
                        {
                            //remove army from game
                            //clear all used territories
                        }
                        deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.PreviousAttemptFailed);
                    }
                }
            }
        }

        private async Task<DeployArmyResponse> askForDeployLocationAsync(Player currentPlayer, DeploymentStatus deploymentStatus)
        {
            var deployArmyRequest = new DeployArmyRequest {
                Board = game.Board.Territories,
                Status = deploymentStatus,
                ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
            };
            var deployArmyResponse = await (await client.PostAsJsonAsync($"{currentPlayer.CallbackAddress}/deployArmy", deployArmyRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<DeployArmyResponse>();
            return deployArmyResponse;
        }

        private async Task doBattle()
        {
            //logic to determine whether or not we keep doing battle
            foreach (var currentPlayer in game.Players)
            {
                var beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.YourTurn);

                var failedTries = 0;
                //check that this location exists and is available to be used (e.g. not occupied by another army)

                while (game.AttackOwnershipValid(currentPlayer.Token, beginAttackResponse.From, beginAttackResponse.To) is false)
                {
                    failedTries++;
                    if (failedTries == MaxFailedTries)
                    {
                        //remove army from game
                        //clear all used territories
                    }
                    beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.PreviousAttackRequestFailed);
                }

                //roll the dice and see how many armies from each side die
                //(this logic should be in the game object)

                //if they still have more armies, ask if they want to continue attacking...
                //(this logic goes here in the game runner)
            }
        }

        private async Task<BeginAttackResponse> askForAttackLocationAsync(Player player, BeginAttackStatus beginAttackStatus )
        {
            var beginAttackRequest = new BeginAttackRequest {
                Board = game.Board.Territories,
                Status = beginAttackStatus
            };
            return await (await client.PostAsJsonAsync($"{player.CallbackAddress}/beginAttack", beginAttackRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<BeginAttackResponse>();
        }

        private Task reportWinner()
        {
            throw new NotImplementedException();
        }

        public bool IsAllArmiesPlaced()
        {

            int playersWithNoRemaining = game.Players.Where(p => game.GetPlayerRemainingArmies(p.Token) == 0).Count();

            if (playersWithNoRemaining == game.Players.Count())
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public void RemovePlayerFromBoard(String token)
        {
            foreach (Territory territory in game.Board.Territories)
            {
                if (territory.Owner == game.getPlayer(token))
                {
                    territory.Owner = null;
                    territory.Armies = 0;
                }
            }
        }

        public void BootPlayerFromGame(string Token)
        {
            RemovePlayerFromBoard(Token);
            game.RemovePlayer(Token);
        }

        public void AfterXFailedContactAttemptsBootPlayer(String Token) 
        {
            if (PlayerFailedConsecutiveAttempts[Token] == MaxFailedTries)
            {
                BootPlayerFromGame(Token);
            }
        }
    }
}
