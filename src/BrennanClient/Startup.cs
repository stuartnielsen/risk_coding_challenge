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
using Westwind.AspNetCore.LiveReload;

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
            services.AddLiveReload();
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddSingleton<ColorGenerator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseLiveReload();
            }

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            var httpClient = httpClientFactory.CreateClient();
            JoinServer(httpClient, Configuration["GameServer"], Configuration["ClientCallbackAddress"], Configuration["PlayerName"]);
        }

        private async Task JoinServer(HttpClient httpClient, string serverName, string clientBaseAddress, string userName)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            var joinRequest = new JoinRequest { CallbackBaseAddress = clientBaseAddress, Name = userName };
            var joinResponse = await httpClient.PostAsJsonAsync($"{serverName}/join", joinRequest);
        }
    }
}
