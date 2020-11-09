using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Risk.Game;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly HttpClient client;
        private readonly Game.Game game;
        public const int MaxFailedTries = 5;

        public GameRunner(HttpClient client, Game.Game game)
        {
            this.client = client;
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
            
            while (game.gameState == GameState.Attacking)
            {
                for(int i = 0; i < game.Players.Count(); i++)
                //foreach (var currentPlayer in game.Players)
                {
                    var beginAttackResponse = await askForAttackLocationAsync(game.Players.ElementAt(i), BeginAttackStatus.YourTurn);

                    var failedTries = 0;
                    //check that this location exists and is available to be used (e.g. not occupied by another army)

                    var attackingTerritory = new Territory(beginAttackResponse.From);
                    var defendingTerritory = new Territory(beginAttackResponse.To);
                    while (game.AttackOwnershipValid(game.Players.ElementAt(i).Token, beginAttackResponse.From, beginAttackResponse.To) is false 
                        || !game.EnoughArmiesToAttack(attackingTerritory) 
                        || !game.Board.GetNeighbors(attackingTerritory).ToList().Contains(defendingTerritory))
                    {
                        failedTries++;
                        if (failedTries == MaxFailedTries)
                        {
                            RemovePlayerFromBoard(game.Players.ElementAt(i).Token);
                            game.RemovePlayerFromGame(game.Players.ElementAt(i).Token);
                            i--;
                        }
                        beginAttackResponse = await askForAttackLocationAsync(game.Players.ElementAt(i), BeginAttackStatus.PreviousAttackRequestFailed);
                    }
                    var continueResponse = new ContinueAttackResponse();

                    do
                    {
                        game.RollDice(beginAttackResponse);
                        if (attackingTerritory.Armies > 1)
                            continueResponse = await askContinueAttackingAsync(game.Players.ElementAt(i));
                        else
                            continueResponse.ContinueAttacking = false;
                    } while (continueResponse.ContinueAttacking);
                }
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

        
        private async Task<ContinueAttackResponse> askContinueAttackingAsync(Player currentPlayer)
        {
            var continueAttackingRequest = new ContinueAttackRequest {
                Board = game.Board.Territories
            };
            var continueAttackingResponse = await (await client.PostAsJsonAsync($"{currentPlayer.CallbackAddress}/continueAttacking", continueAttackingRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<ContinueAttackResponse>();
            return continueAttackingResponse;
        }
    }
}
