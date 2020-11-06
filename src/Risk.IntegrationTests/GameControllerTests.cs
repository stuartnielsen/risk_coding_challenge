using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Risk.Shared;
using Xunit;

namespace Risk.IntegrationTests
{
    public class GameControllerTests : IClassFixture<WebApplicationFactory<Risk.Api.Startup>>
    {
        private HttpClient httpClient;
        private readonly WebApplicationFactory<Risk.Api.Startup> factory;

        public GameControllerTests(WebApplicationFactory<Risk.Api.Startup> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task CanHitGameStatus()
        {
            httpClient = factory.CreateClient();

            var response = await httpClient.GetAsync("/GameStatus");

            Assert.True(response.IsSuccessStatusCode);


        }

        [Fact]
        public async Task CanGetGameStatusWhileInJoining()
        {
            httpClient = factory.CreateClient();

            var response = await httpClient.GetAsync("/GameStatus");

        }

        [Fact]
        public async Task CanOnlyJoinWhenInJoining()
        {
            httpClient = factory.CreateClient();
        }
    }
}
