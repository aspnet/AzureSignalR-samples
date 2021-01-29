// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSignalR()
                    .AddAzureSignalR(options =>
                    {
                        options.ClaimsProvider = context => new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
                        };
                    });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseFileServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ReliableChatRoom>("/chat");
            }
            );
        }
    }
}
