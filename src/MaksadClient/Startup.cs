using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Risk.Shared;
using System.Net.Http.Json;
using Westwind.AspNetCore.LiveReload;


namespace MaksadClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpClient();
            services.AddLiveReload();
            services.AddRazorPages().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseLiveReload();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            var server = Configuration["ServerName"];
            var httpClient = httpClientFactory.CreateClient();
            var clientBaseAddress = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First(a => a.Contains("http:"));

            JoinServer(httpClient, server, clientBaseAddress);

        }

        private async Task JoinServer(HttpClient httpClient, string serverName, string clientBaseAddress)
        {
            var joinRequest = new JoinRequest { CallbackBaseAddress = clientBaseAddress, Name = "Maksad" };

            var joinResponse = await httpClient.PostAsJsonAsync($"{serverName}/join", joinRequest);

        }
    }
}
