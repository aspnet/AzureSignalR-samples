// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Timer
{
    public static class TimerFunction
    {
        public static async Task Run(TimerInfo myTimer, TraceWriter log)
        {
            var serviceContext = AzureSignalR.CreateServiceContext("chat");
            await serviceContext.HubContext.Clients.All.SendAsync("broadcastMessage", "_BROADCAST_",
                $"Current time is: {DateTime.Now}");
        }
    }
}
