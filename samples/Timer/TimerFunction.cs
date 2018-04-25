// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;

namespace Timer
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.SignalR;

    public static class TimerFunction
    {
        public static async Task Run(TimerInfo myTimer, TraceWriter log)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureSignalRConnectionString");
            var serviceContext = AzureSignalR.CreateServiceContext(connectionString, "chat");
            await serviceContext.HubContext.Clients.All.SendAsync("broadcastMessage",
                new object[]
                {
                    "_BROADCAST_", $"Current time is: {DateTime.Now}"
                });
        }
    }
}
