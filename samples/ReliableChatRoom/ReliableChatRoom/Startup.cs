// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage;
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
            services.AddSingleton<IUserHandler, UserHandler>();
            services.AddSingleton<IMessageStorage, InMemoryStorage>();
            services.AddSingleton<IMessageFactory, MessageFactory>();
            services.AddSingleton<IClientAckHandler, ClientAckHandler>();
            services.AddSingleton<INotificationHandler, NotificationHandler>(provider => new NotificationHandler(provider.GetService<IUserHandler>(), Configuration["Azure:NotificationHub:ConnectionString"], Configuration["Azure:NotificationHub:HubName"]));
            services.AddSingleton<IPersistentStorage, BlobPersistentStorage>(provider => new BlobPersistentStorage(provider.GetService<IMessageFactory>(), Configuration["Azure:Storage:ConnectionString"]));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ReliableChatRoomHub>("/chat");
            });
        }
    }
}
