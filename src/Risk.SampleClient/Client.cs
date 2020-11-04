using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Api.Controllers;
using Risk.Shared;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace Risk.SampleClient
{
    public class Client
    {
        public Client (IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private readonly IHttpClientFactory _httpClientFactory;

        string playerName { get; set; }
        Uri callback { get; set; }


        public static HttpRequest Request { get; }

        string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
        public async Task OnGet (string playerName )
        {
            var client = _httpClientFactory.CreateClient("RiskApi");
            var parameters = new Dictionary<string, string> { { "playerName", playerName },{ "callback", baseUrl.ToString()}};
            var encodedContent = new FormUrlEncodedContent(parameters);
            await client.PostAsync("server/join", encodedContent);
        }
    }
}
