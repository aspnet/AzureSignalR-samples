// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFlightControl, FlightControl>();
            services.AddMvc();
            if (Configuration["UseLocal"] != "true") services.AddSignalR().AddAzureSignalR();
            else services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseFileServer();
            if (Configuration["UseLocal"] != "true")
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<FlightMapHub>("/flightData");
                });
            else
                app.UseSignalR(routes =>
                { 
                    routes.MapHub<FlightMapHub>("/flightData");
                });
        }
    }
}
