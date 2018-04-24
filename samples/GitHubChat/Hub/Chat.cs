// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class Chat : Hub
    {
        public override Task OnConnectedAsync()
        {
            return Clients.All.SendAsync("broadcastMessage", "_SYSTEM_", $"{Context.UserIdentifier} JOINED");
        }

        // Uncomment this line to only allow user in Microsoft to send message
        // [Authorize(Policy = "Microsoft_Only")]
        public void BroadcastMessage(string message)
        {
            Clients.All.SendAsync("broadcastMessage", Context.UserIdentifier, message);
        }

        public void Echo(string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", Context.UserIdentifier,
                message + " (echo from server)");
        }
    }
}
