using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RiskClientController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("/")]
        public string Info()
        {
            return "Submit a get request to /joinServer/{serverAddress} to join a game.";
        }

        [HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> Join(string server)
        {
            var client = httpClientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "braindead client"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{server}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }
    }
}
