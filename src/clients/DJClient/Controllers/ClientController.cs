using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DJClient.Controllers
{
    
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpGet("AreYouThere")] 
        public string AreYouThere( )
        {
            return "yes";
        }
    }
}
