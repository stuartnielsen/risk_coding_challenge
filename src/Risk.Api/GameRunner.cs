using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Risk.Api
{
    public class GameRunner
    {

        private readonly Game.Game game;
        private readonly IHttpClientBuilder httpBuilder;

        public GameRunner(Game.Game game, IHttpClientBuilder httpBuilder)
        {
            this.httpBuilder = httpBuilder;
            this.httpBuilder = httpBuilder;
        }


        public bool IsAllArmiesPlaced()
        {
            throw new NotImplementedException();
        }
    }
}
