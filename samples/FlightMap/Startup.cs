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
            services.AddControllers();
            services.AddSignalR().AddAzureSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseFileServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<FlightMapSampleHub>("/flightData");
            });
        }
    }
}
