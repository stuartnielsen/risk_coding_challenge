using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Risk.Shared;

namespace BrennanClient
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var server = Configuration["ServerName"];
            var httpClient = httpClientFactory.CreateClient();
            var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
            var clientBaseAddress = addresses.First();
            JoinServer(httpClient, Configuration["GameServer"], Configuration["ClientCallbackAddress"], Configuration["userName"]);
        }

        private async Task JoinServer(HttpClient httpClient, string serverName, string clientBaseAddress, string userName)
        {
            var joinRequest = new JoinRequest { CallbackBaseAddress = clientBaseAddress, Name = userName };

            var joinResponse = await httpClient.PostAsJsonAsync($"{serverName}/join", joinRequest);

        }
    }
}
