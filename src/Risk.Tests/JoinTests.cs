using System;
using System.Collections.Generic;
using System.Text;
using Risk.Shared;
using Risk.Game;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;

namespace Risk.Tests
{
    class JoinTests
    {

        private Game.Game game;

        [SetUp]
        public void Setup()
        {

            game = new Game.Game(new GameStartOptions { Height = 5, Width = 5, StartingArmiesPerPlayer = 5 });
            game.StartJoining();
        }
    
        [Test]
        public void TestToSeeIfPlayerJoined()
        {
            string token = game.AddPlayer("Random");
            Assert.IsNotNull(game.Players.Single(p => p.Token == token));
        }
    }
}

