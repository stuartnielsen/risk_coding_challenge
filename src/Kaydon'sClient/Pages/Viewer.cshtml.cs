using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Kaydon_sClient.Pages
{
    public class ViewerModel : PageModel
    {
        private IHttpClientFactory httpClientFactory;
        private IConfiguration configuration;
        public GameStatus GameStatus { get; set; }
        public int MaxRow { get; set; }
        public int MaxCol { get; set; }
        public ViewerModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }
        public async Task OnGetAsync()
        {
            GameStatus = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
            MaxRow = GameStatus.Board.Max(t => t.Location.Row);
            MaxCol = GameStatus.Board.Max(t => t.Location.Column);
        }
    }
}
