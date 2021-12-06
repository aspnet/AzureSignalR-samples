// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddSignalR(options =>
                    {
                        options.MaximumReceiveMessageSize = 1024 * 1024 * 1024;
                    })
                    .AddAzureSignalR();
            services.AddSingleton<IUserHandler, UserHandler>();
            services.AddSingleton<IMessageFactory, MessageFactory>();
            services.AddSingleton<IClientAckHandler, ClientAckHandler>();
            services.AddSingleton<INotificationHandler, NotificationHandler>(provider => new NotificationHandler(provider.GetService<ILogger<NotificationHandler>>(), provider.GetService<IUserHandler>(), Configuration["ConnectionStrings:AzureNotificationHub:ConnectionString"], Configuration["ConnectionStrings:AzureNotificationHub:HubName"]));
            services.AddSingleton<IMessageStorage, AzureTableMessageStorage>(provider => new AzureTableMessageStorage(provider.GetService<ILogger<AzureTableMessageStorage>>(), provider.GetService<IMessageFactory>(), Configuration["ConnectionStrings:AzureStorageAccountConnectionString"]));
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
