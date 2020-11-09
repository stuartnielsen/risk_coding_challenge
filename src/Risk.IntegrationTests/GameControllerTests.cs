using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Risk.Shared;

namespace Risk.IntegrationTests
{
    public class GameControllerTests
    {
        [Test]
        public async Task GetStatus_WithoutGameStarted_ShowsNothing()
        {
            //var joinRequest = new JoinRequest {
            //    CallbackBaseAddress = "http://localhost:6001",
            //    Name = "in-memory client"
            //};
            //var joinResponse = await (await serverClient.PostAsJsonAsync("/join", joinRequest)).Content.ReadAsStringAsync();

            //var status = serverClient.GetAsync("/status");
        }
    }
}
