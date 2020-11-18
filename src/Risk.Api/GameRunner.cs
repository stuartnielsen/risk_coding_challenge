using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Risk.Game;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {
        private readonly Game.Game game;
        private readonly IList<ApiPlayer> players;
        private readonly IList<ApiPlayer> removedPlayers;
        private readonly ILogger<GameRunner> logger;
        public const int MaxFailedTries = 5;

        public GameRunner(Game.Game game, IList<ApiPlayer> players, IList<ApiPlayer> removedPlayers, ILogger<GameRunner> logger)
        {
            this.game = game;
            this.players = players;
            this.removedPlayers = removedPlayers;
            this.logger = logger;
        }

        public async Task StartGameAsync()
        {
            await deployArmiesAsync();
            await doBattle();
            await reportWinner();
        }

        private async Task deployArmiesAsync()
        {
            while (game.Board.Territories.Sum(t => t.Armies) < game.StartingArmies * players.Count())
            {
                for (int playerIndex = 0; playerIndex < players.Count(); ++playerIndex)
                {
                    var currentPlayer = players[playerIndex];
                    var deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.YourTurn);

                    var failedTries = 0;
                    //check that this location exists and is available to be used (e.g. not occupied by another army)
                    while (game.TryPlaceArmy(currentPlayer.Token, deployArmyResponse.DesiredLocation) is false)
                    {
                        failedTries++;
                        if (failedTries == MaxFailedTries)
                        {
                            BootPlayerFromGame(currentPlayer);
                            playerIndex--;
                        }
                        else
                        {
                            deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.PreviousAttemptFailed);
                        }
                    }
                }
            }
        }

        private async Task<DeployArmyResponse> askForDeployLocationAsync(ApiPlayer currentPlayer, DeploymentStatus deploymentStatus)
        {
            var deployArmyRequest = new DeployArmyRequest {
                Board = game.Board.SerializableTerritories,
                Status = deploymentStatus,
                ArmiesRemaining = game.GetPlayerRemainingArmies(currentPlayer.Token)
            };
            var json = System.Text.Json.JsonSerializer.Serialize(deployArmyRequest);
            var deployArmyResponse = (await currentPlayer.HttpClient.PostAsJsonAsync("/deployArmy", deployArmyRequest));
            deployArmyResponse.EnsureSuccessStatusCode();
            var r = await deployArmyResponse.Content.ReadFromJsonAsync<DeployArmyResponse>();
            return r;
        }

        private async Task doBattle()
        {
            game.StartTime = DateTime.Now;
            while (players.Count > 1 && game.GameState == GameState.Attacking)
            {
                bool someonePlayedThisRound = false;

                for(int i = 0; i < players.Count; i++)
                {
                    var currentPlayer = players[i];
                    if (game.PlayerCanAttack(currentPlayer))
                    {
                        someonePlayedThisRound = true;
                        var failedTries = 0;

                        TryAttackResult attackResult;
                        Territory attackingTerritory;
                        Territory defendingTerritory;
                        do
                        {
                            logger.LogInformation($"Asking {currentPlayer.Name} where they want to attack...");

                            var beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.PreviousAttackRequestFailed);
                            attackingTerritory = game.Board.GetTerritory(beginAttackResponse.From);
                            defendingTerritory = game.Board.GetTerritory(beginAttackResponse.To);

                            logger.LogInformation($"{currentPlayer.Name} wants to attack from {attackingTerritory} to {defendingTerritory}");

                            attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);

                            if (attackResult.AttackInvalid)
                            {
                                logger.LogError("Invalid attack request!");
                                failedTries++;
                                if (failedTries == MaxFailedTries)
                                {
                                    BootPlayerFromGame(currentPlayer);
                                    RemovePlayerFromBoard(currentPlayer.Token);
                                    RemovePlayerFromGame(currentPlayer.Token);
                                    i--;
                                    break;
                                }
                            }
                        } while (attackResult.AttackInvalid);

                        while(attackResult.CanContinue)
                        {
                            var continueResponse = await askContinueAttackingAsync(currentPlayer, attackingTerritory, defendingTerritory);
                            if (continueResponse.ContinueAttacking)
                            {
                                logger.LogInformation("Keep attacking!");
                                attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);
                            }
                            else
                            {
                                logger.LogInformation("run away!");
                                break;
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning($"{currentPlayer.Name} cannot attack.");
                    }
                }

                if(someonePlayedThisRound is false)
                {
                    logger.LogInformation("Game Over");
                    game.SetGameOver();
                    return;
                }
            }
        }

        private void RemovePlayerFromGame(string token)
        {
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                if (player.Token == token)
                {
                    players.Remove(player);
                    removedPlayers.Add(player);
                }
            }
        }

        private async Task<BeginAttackResponse> askForAttackLocationAsync(ApiPlayer player, BeginAttackStatus beginAttackStatus)
        {
            var beginAttackRequest = new BeginAttackRequest {
                Board = game.Board.SerializableTerritories,
                Status = beginAttackStatus
            };
            return await (await player.HttpClient.PostAsJsonAsync("/beginAttack", beginAttackRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<BeginAttackResponse>();
        }

        private async Task reportWinner()
        {
            game.EndTime = DateTime.Now;
            TimeSpan gameDuration = game.EndTime - game.StartTime;

            var scores = new List<(int, ApiPlayer)>();

            foreach (var currentPlayer in players)
            {
                var playerScore = 2 * game.GetNumTerritories(currentPlayer) + game.GetNumPlacedArmies(currentPlayer);

                scores.Add((playerScore, currentPlayer));
            }

            scores.Sort();

            foreach (var currentPlayer in players)
            {
                await sendGameOverRequest(currentPlayer, gameDuration, scores);
            }
        }

        private async Task sendGameOverRequest(ApiPlayer player, TimeSpan gameDuration, List<(int score, ApiPlayer player)> scores)
        {
            var gameOverRequest = new GameOverRequest {
                FinalBoard = game.Board.SerializableTerritories,
                GameDuration = gameDuration.ToString(),
                WinnerName = scores.Last().player.Name,
                FinalScores = scores.Select(s => $"{s.player.Name} ({s.score})")
            };

            var response = await (player.HttpClient.PostAsJsonAsync("/gameOver", gameOverRequest));
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
                if (territory.Owner == game.GetPlayer(token))
                {
                    territory.Owner = null;
                    territory.Armies = 0;
                }
            }
        }

        private async Task<ContinueAttackResponse> askContinueAttackingAsync(ApiPlayer currentPlayer, Territory attackingTerritory, Territory defendingTerritory)
        {
            var continueAttackingRequest = new ContinueAttackRequest {
                Board = game.Board.SerializableTerritories,
                AttackingTerritorry = attackingTerritory,
                DefendingTerritorry = defendingTerritory
            };
            var continueAttackingResponse = await (await currentPlayer.HttpClient.PostAsJsonAsync("/continueAttacking", continueAttackingRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<ContinueAttackResponse>();
            return continueAttackingResponse;
        }

        public void BootPlayerFromGame(ApiPlayer player)
        {
            RemovePlayerFromBoard(player.Token);
            players.Remove(player);
        }


    }
}
