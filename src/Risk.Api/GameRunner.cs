using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly HttpClient client;
        private readonly Game.Game game;
        public const int MaxFailedTries = 5;

        public GameRunner(IHttpClientFactory clientFactory, Game.Game game)
        {
            this.client = clientFactory.CreateClient();
            this.game = game;
        }

        public async Task StartGameAsync()
        {
            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
        }

        private async Task deployArmiesAsync()
        {
            while(game.Board.Territiories.Sum(t=>t.Armies) < game.StartingArmies * game.Players.Count())
            {
                foreach (var currentPlayer in game.Players)
                {
                    var deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.YourTurn);

                    var failedTries = 0;
                    //check that this lcoation exisits and is available to be used (e.g. not occupied by another army)
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
                Board = game.Board.Territiories,
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
            foreach (var currentPlayer in game.Players)
            {
                var beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.YourTurn);

                var failedTries = 0;
                //check that this lcoation exisits and is available to be used (e.g. not occupied by another army)
                while (game.AttackOwnershipValid(currentPlayer.Token, beginAttackResponse.From, beginAttackResponse.To) is false)
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
        private async Task<BeginAttackResponse> askForAttackLocationAsync(Player player, BeginAttackStatus beginAttackStatus )
        {
            var beginAttackRequest = new BeginAttackRequest {
                Board = game.Board.Territiories,
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
    }
}
