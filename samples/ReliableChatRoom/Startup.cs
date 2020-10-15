// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
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
            services.AddSignalR()
                    .AddAzureSignalR();
            services.AddSingleton<IMessageHandler, StaticMessageStorage>();
            services.AddSingleton<IAckHandler, AckHandler>();
            services.AddSingleton<ILoginHandler, LoginHandler>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ReliableRoamingChatroomHub>("/chat");
            });
        }
    }
}
