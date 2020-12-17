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
        private readonly IList<ApiPlayer> removedPlayers;
        private readonly ILogger<GameRunner> logger;
        public const int MaxFailedTries = 5;

        public GameRunner(Game.Game game, ILogger<GameRunner> logger)
        {
            this.game = game;
            this.removedPlayers = new List<ApiPlayer>();
            this.logger = logger;
        }

        public async Task StartGameAsync()
        {
            await deployArmiesAsync();
            await newDoBattle();
            await reportWinner();
        }

        private async Task deployArmiesAsync()
        {
            while (game.Board.Territories.Sum(t => t.Armies) < game.StartingArmies * game.Players.Count())
            {
                for (int playerIndex = 0; playerIndex < game.Players.Count(); ++playerIndex)
                {
                    var currentPlayer = game.Players.Skip(playerIndex).First() as ApiPlayer;
                    var deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.YourTurn);
                    var failedTries = 0;
                    //check that this location exists and is available to be used (e.g. not occupied by another army)
                    while (game.TryPlaceArmy(currentPlayer.Token, deployArmyResponse.DesiredLocation) is false)
                    {
                        failedTries++;
                        if (failedTries == MaxFailedTries)
                        {
                            BootPlayerFromGame(currentPlayer);
                            logger.LogError($"Player {currentPlayer.Name} was booted from game due to deploy error");
                            playerIndex--;
                            break;
                        }
                        else
                        {
                            deployArmyResponse = await askForDeployLocationAsync(currentPlayer, DeploymentStatus.PreviousAttemptFailed);
                        }
                    }
                    logger.LogDebug($"{currentPlayer.Name} wants to deploy to {deployArmyResponse.DesiredLocation}");
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

        private async Task<DeployArmyResponse> askForReinforceLocationAsync(ApiPlayer currentPlayer, DeploymentStatus deploymentStatus, int armiesRemaining)
        {
            var reinforceRequest = new DeployArmyRequest {
                Board = game.Board.SerializableTerritories,
                Status = deploymentStatus,
                ArmiesRemaining = armiesRemaining
            };
            var json = System.Text.Json.JsonSerializer.Serialize(reinforceRequest);
            var deployArmyResponse = (await currentPlayer.HttpClient.PostAsJsonAsync("/reinforce", reinforceRequest));
            deployArmyResponse.EnsureSuccessStatusCode();
            var r = await deployArmyResponse.Content.ReadFromJsonAsync<DeployArmyResponse>();
            return r;
        }

        private async Task newDoBattle()
        {
            game.StartTime = DateTime.Now;
            int CardBonusCount = 0;
            while (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
            {
                for (int i = 0; i < game.Players.Count() && game.Players.Count() > 1; i++)
                {

                    var currentPlayer = game.Players.Skip(i).First() as ApiPlayer;
                    if (game.GetNumTerritories(currentPlayer) > 0)
                    {
                        var usedCardBonus = await DeployPlayerArmies(currentPlayer, CardBonusCount);
                        if (usedCardBonus)
                            CardBonusCount++;

                        if (!removedPlayers.Contains(currentPlayer))
                            await DoPlayerBattle(currentPlayer);
                        if (!removedPlayers.Contains(currentPlayer))
                            await PlayerManeuver(currentPlayer);
                        else i--;
                    }

                }
            }
            logger.LogInformation("Game Over");
            game.SetGameOver();
        }

        /*private async Task doBattle()//old
        {
            game.StartTime = DateTime.Now;
            while (game.Players.Count() > 1 && game.GameState == GameState.Attacking && game.Players.Any(p => game.PlayerCanAttack(p)))
            {

                for (int i = 0; i < game.Players.Count() && game.Players.Count() > 1; i++)
                {
                    var currentPlayer = game.Players.Skip(i).First() as ApiPlayer;
                    if (game.PlayerCanAttack(currentPlayer))
                    {
                        var failedTries = 0;

                        TryAttackResult attackResult = new TryAttackResult { AttackInvalid = false };
                        Territory attackingTerritory = null;
                        Territory defendingTerritory = null;
                        do
                        {
                            logger.LogInformation($"Asking {currentPlayer.Name} where they want to attack...");

                            var beginAttackResponse = await askForAttackLocationAsync(currentPlayer, BeginAttackStatus.PreviousAttackRequestFailed);
                            try
                            {
                                attackingTerritory = game.Board.GetTerritory(beginAttackResponse.From);
                                defendingTerritory = game.Board.GetTerritory(beginAttackResponse.To);

                                logger.LogInformation($"{currentPlayer.Name} wants to attack from {attackingTerritory} to {defendingTerritory}");

                                attackResult = game.TryAttack(currentPlayer.Token, attackingTerritory, defendingTerritory);
                            }
                            catch (Exception ex)
                            {
                                attackResult = new TryAttackResult { AttackInvalid = true, Message = ex.Message };
                            }
                            if (attackResult.AttackInvalid)
                            {
                                logger.LogError($"Invalid attack request! {currentPlayer.Name} from {attackingTerritory} to {defendingTerritory} ");
                                failedTries++;
                                if (failedTries == MaxFailedTries)
                                {
                                    BootPlayerFromGame(currentPlayer);
                                    i--;
                                    break;
                                }
                            }
                        } while (attackResult.AttackInvalid);

                        while (attackResult.CanContinue)
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


            }
            logger.LogInformation("Game Over");
            game.SetGameOver();
        }*/

        private void RemovePlayerFromGame(string token)
        {
            var player = game.RemovePlayerByToken(token) as ApiPlayer;
            removedPlayers.Add(player);
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

            var scores = new List<(int score, ApiPlayer player)>();

            foreach (ApiPlayer currentPlayer in game.Players)
            {
                var playerScore = 2 * game.GetNumTerritories(currentPlayer) + game.GetNumPlacedArmies(currentPlayer);

                scores.Add((playerScore, currentPlayer));
            }

            var orderedScores = scores.OrderByDescending(s => s.score);

            var gameOverRequest = new GameOverRequest {
                FinalBoard = game.Board.SerializableTerritories,
                GameDuration = gameDuration.ToString(),
                WinnerName = orderedScores.First().player.Name,
                FinalScores = orderedScores.Select(s => $"{s.player.Name} ({s.score})")
            };

            foreach (ApiPlayer currentPlayer in game.Players)
            {
                var response = await (currentPlayer.HttpClient.PostAsJsonAsync("/gameOver", gameOverRequest));
            }
        }

        public bool IsAllArmiesPlaced()
        {
            int playersWithNoRemaining = game.Players.Count(p => game.GetPlayerRemainingArmies(p.Token) == 0);

            return (playersWithNoRemaining == game.Players.Count());
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
            RemovePlayerFromGame(player.Token);
        }

        public async Task<bool> DeployPlayerArmies(ApiPlayer player, int cardBonusLevel)
        {
            bool cardBonusUsed = HasCardBonus(player);
            int armiesForTerritories = FindNumberBonusArmiesFromTerritories(player);
            int totalBonusArmies = armiesForTerritories;
            if (cardBonusUsed)
            {
                totalBonusArmies += (((cardBonusLevel + 1) % 19) + 1) * 5;
                logger.LogInformation($"Card Bonus of level: {cardBonusLevel} for {totalBonusArmies} armies total");
            }
            await Reinforce(player, totalBonusArmies);
            return cardBonusUsed;
        }

        public int FindNumberBonusArmiesFromTerritories(ApiPlayer player)
        {
            return game.GetNumTerritories(player) / 3;
        }

        public bool HasCardBonus(ApiPlayer player)
        {
            int infantryCards = 0;
            int cavalryCards = 0;
            int artilleryCards = 0;
            bool hasBonus = false;
            if (player.PlayerCards.Count > 2)
            {
                foreach (var card in player.PlayerCards)
                {
                    if (card.Type == "Infantry")
                        infantryCards++;
                    if (card.Type == "Cavalry")
                        cavalryCards++;
                    if (card.Type == "Artillery")
                        artilleryCards++;
                }

                if (infantryCards > 2)
                {
                    hasBonus = true;
                    for (int i = 0; i < 3; i++)
                        player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Infantry").First());
                }
                else if (cavalryCards > 2)
                {
                    hasBonus = true;
                    for (int i = 0; i < 3; i++)
                        player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Cavalry").First());
                }
                else if (artilleryCards > 2)
                {
                    hasBonus = true;
                    for (int i = 0; i < 3; i++)
                        player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Artillery").First());
                }
                else if (infantryCards > 0 && cavalryCards > 0 && artilleryCards > 0)
                {
                    hasBonus = true;
                    player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Infantry").First());
                    player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Cavalry").First());
                    player.PlayerCards.Remove(player.PlayerCards.Where(c => c.Type == "Artillery").First());

                }
            }
            return hasBonus;
        }
        public async Task Reinforce(ApiPlayer player, int armiesRemaining)
        {
            while (armiesRemaining > 0)
            {
                var reinforceResponse = await askForReinforceLocationAsync(player, DeploymentStatus.YourTurn, armiesRemaining);
                logger.LogDebug($"{player.Name} wants to reinforce to {reinforceResponse.DesiredLocation}");
                var failedTries = 0;
                //check that this location exists and is available to be used (e.g. not occupied by another army)
                while (game.TryReinforceArmy(player.Token, reinforceResponse.DesiredLocation) is false)
                {
                    failedTries++;
                    logger.LogError($"Invalid Reinforce request! {player.Name} to {reinforceResponse.DesiredLocation}");
                    if (failedTries == MaxFailedTries)
                    {
                        BootPlayerFromGame(player);
                        logger.LogError($"Player {player.Name} Booted! Due to reinforce error");
                        return;
                    }
                    else
                    {
                        reinforceResponse = await askForReinforceLocationAsync(player, DeploymentStatus.PreviousAttemptFailed, armiesRemaining);
                    }
                }
                armiesRemaining--;
            }
        }

        public async Task DoPlayerBattle(ApiPlayer player)
        {
            if (game.PlayerCanAttack(player))
            {
                var failedTries = 0;
                bool hasCard = false;
                ContinueAttackResponse anotherAttackResponse = new ContinueAttackResponse { ContinueAttacking = false };
                do
                {
                    TryAttackResult attackResult = new TryAttackResult { AttackInvalid = false };
                    Territory attackingTerritory = null;
                    Territory defendingTerritory = null;


                    do
                    {
                        logger.LogInformation($"Asking {player.Name} where they want to attack...");

                        var beginAttackResponse = await askForAttackLocationAsync(player, BeginAttackStatus.PreviousAttackRequestFailed);
                        try
                        {
                            attackingTerritory = game.Board.GetTerritory(beginAttackResponse.From);
                            defendingTerritory = game.Board.GetTerritory(beginAttackResponse.To);

                            logger.LogInformation($"{player.Name} wants to attack from {attackingTerritory} to {defendingTerritory}");

                            attackResult = game.TryAttack(player.Token, attackingTerritory, defendingTerritory);

                            if (attackResult.BattleWasWon && !hasCard)
                            {
                                player.PlayerCards.Add(new Card());
                                hasCard = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            attackResult = new TryAttackResult { AttackInvalid = true, Message = ex.Message };
                        }
                        if (attackResult.AttackInvalid)
                        {
                            logger.LogError($"Invalid attack request! {player.Name} from {attackingTerritory} to {defendingTerritory} ");
                            failedTries++;
                            if (failedTries == MaxFailedTries)
                            {
                                BootPlayerFromGame(player);
                                logger.LogError($"Player {player.Name} Booted! Due to attack error");
                                return;
                            }
                        }
                    } while (attackResult.AttackInvalid);
                    while (attackResult.CanContinue)
                    {
                        var continueResponse = await askContinueAttackingAsync(player, attackingTerritory, defendingTerritory);
                        if (continueResponse.ContinueAttacking)
                        {
                            logger.LogInformation("Keep attacking!");
                            attackResult = game.TryAttack(player.Token, attackingTerritory, defendingTerritory);
                            if (attackResult.BattleWasWon && !hasCard)
                            {
                                player.PlayerCards.Add(new Card());
                                hasCard = true;
                            }
                        }
                        else
                        {
                            logger.LogInformation("run away!");
                            break;
                        }
                    }

                    if (game.PlayerCanAttack(player))
                        anotherAttackResponse = await AskMakeAnotherAttackAsync(player, attackingTerritory, defendingTerritory);
                    else
                        anotherAttackResponse.ContinueAttacking = false;
                } while (anotherAttackResponse.ContinueAttacking);
            }
            else
            {
                logger.LogWarning($"{player.Name} cannot attack.");
            }
        }

        public async Task PlayerManeuver(ApiPlayer player)
        {
            var response = await askForManeuverkLocationAsync(player);
            if (response.Decide is false)
            {
                return;
            }
            var fromTerritory = game.Board.GetTerritory(response.From);
            var toTerritory = game.Board.GetTerritory(response.To);
            Boolean result = new Boolean();
            var failedTrys = 0;
            do
            {
                logger.LogInformation($"{player.Name} wants to maneuver from {fromTerritory} to {toTerritory}");
                result = game.TryManeuver(player, fromTerritory, toTerritory);
                if (!result)
                {
                    logger.LogError($"Invalid maneuver request! {player.Name} from {fromTerritory} to {toTerritory} ");
                    failedTrys++;
                }
                if (failedTrys >= 3)
                {
                    BootPlayerFromGame(player);
                    logger.LogError($"Player {player.Name} Booted! Due to maneuver error.");
                    return;
                }
            } while (!result);

        }
        private async Task<ManeuverResponse> askForManeuverkLocationAsync(ApiPlayer player)
        {
            var maneuverRequest = new ManeuverRequest {
                Board = game.Board.SerializableTerritories
            };
            return await (await player.HttpClient.PostAsJsonAsync("/maneuver", maneuverRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<ManeuverResponse>();
        }

        private async Task<ContinueAttackResponse> AskMakeAnotherAttackAsync(ApiPlayer player, Territory attackingTerritory, Territory defendingTerritory)
        {
            var newAttackRequest = new ContinueAttackRequest {
                Board = game.Board.SerializableTerritories,
                AttackingTerritorry = attackingTerritory,
                DefendingTerritorry = defendingTerritory
            };
            return await (await player.HttpClient.PostAsJsonAsync("/makeNewAttack", newAttackRequest))
                .EnsureSuccessStatusCode()
                .Content.ReadFromJsonAsync<ContinueAttackResponse>();
        }
    }
}
