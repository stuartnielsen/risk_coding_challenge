using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly Game.Game game;
        private readonly IEnumerable<ApiPlayer> players;
        public const int MaxFailedTries = 5;

        public GameRunner(Game.Game game, IEnumerable<ApiPlayer> players)
        {
            this.game = game;
            this.players = players;
        }

        public async Task StartGameAsync()
        {
            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
        }

        private async Task deployArmiesAsync()
        {
            while(game.Board.Territories.Sum(t=>t.Armies) < game.StartingArmies * players.Count())
            {
                foreach (var currentPlayer in players)
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

        private async Task<DeployArmyResponse> askForDeployLocationAsync(ApiPlayer currentPlayer, DeploymentStatus deploymentStatus)
        {
            var deployArmyRequest = new DeployArmyRequest {
                Board = game.Board.Territories,
                Status = deploymentStatus,
                ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
            };
            var deployArmyResponse = await (await currentPlayer.HttpClient.PostAsJsonAsync("/deployArmy", deployArmyRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<DeployArmyResponse>();
            return deployArmyResponse;
        }

        private async Task doBattle()
        {
            game.startTime = DateTime.Now;
            //logic to determine whether or not we keep doing battle
            foreach (var currentPlayer in players)
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

        private async Task<BeginAttackResponse> askForAttackLocationAsync(ApiPlayer player, BeginAttackStatus beginAttackStatus )
        {
            var beginAttackRequest = new BeginAttackRequest {
                Board = game.Board.Territories,
                Status = beginAttackStatus
            };
            return await (await player.HttpClient.PostAsJsonAsync("/beginAttack", beginAttackRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<BeginAttackResponse>();
        }

        private async Task reportWinner()
        {
            game.endTime = DateTime.Now;
            TimeSpan gameDuration = game.endTime - game.startTime;

            var scores = new List<(int, ApiPlayer)>();

            foreach (var currentPlayer in players)
            {
                var playerScore = 2 * game.getNumTerritories(currentPlayer) + game.getNumPlacedArmies(currentPlayer);

                foreach(var player in scores)
                {

                }

                scores.Add((playerScore, currentPlayer));

                await sendGameOverRequest(currentPlayer, gameDuration, scores);
            }

        }


        private Task sendGameOverRequest(ApiPlayer player, TimeSpan gameDuration, List<(int, ApiPlayer)> scores)
        {
            scores.Sort();
            
            var gameOverRequest = new GameOverRequest {
                FinalBoard = game.Board.Territories,
                GameDuration = gameDuration,
                WinnerName = scores.Last().Item2.Name,
                FinalScores = scores.ToDictionary(x => x.Item1, x => x.Item2.Name)
            };

            return (player.HttpClient.PostAsJsonAsync("/gameOver", gameOverRequest));
               
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
    }
}
