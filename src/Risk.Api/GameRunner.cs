using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Risk.Shared;

namespace Risk.Api
{
    public class GameRunner
    {

        private Game.Game game;
        private IHttpClientBuilder httpBuilder;

        public GameRunner(Game.Game game, IHttpClientBuilder httpBuilder)
        {
            this.game = game;
            this.httpBuilder = httpBuilder;
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
