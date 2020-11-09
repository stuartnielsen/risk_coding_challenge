using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Risk.Shared;

namespace Risk.IntegrationTests
{
    public class ServerApiTests
    {

        private readonly WebApplicationFactory<Risk.Api.Startup> serverWebAppFactory;
        private readonly WebApplicationFactory<Risk.SampleClient.Startup> clientWebAppFactory;

        private HttpClient serverHttpClient;
        private HttpClient clientHttpClient;

        public ServerApiTests(/*WebApplicationFactory<Risk.Api.Startup> webApplicationFactory*/)
        {
            serverWebAppFactory = new WebApplicationFactory<Api.Startup>();
            clientWebAppFactory = new WebApplicationFactory<SampleClient.Startup>();
        }

        [SetUp]
        public void Setup()
        {
            serverHttpClient = serverWebAppFactory.CreateClient();
            clientHttpClient = clientWebAppFactory.CreateClient();
        }

        [Test]
        public async Task CanGetGameStatus()
        {
            var result = await serverHttpClient.GetAsync("GameStatus");

            result.StatusCode.IsSameOrEqualTo(HttpStatusCode.OK);
        }

        [Test]
        public async Task CanStartGame()
        {
            //ToDo: Find a way to get the secret code from Api.appsetting.json
            var response = await serverHttpClient.PostAsJsonAsync("StartGame", new StartGameRequest { SecretCode = "BazookaJoe" });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task CannotJoinAfterStartingGame()
        {
            var joinRequest = new JoinRequest { CallbackBaseAddress = clientHttpClient.BaseAddress.ToString(), Name = "client" };

            await serverHttpClient.PostAsJsonAsync("StartGame", new StartGameRequest { SecretCode = "BazookaJoe" });


            var response = await serverHttpClient.PostAsJsonAsync("Join", joinRequest);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}