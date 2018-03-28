// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.Authorization;

    public class Chat : Hub
    {
        public override Task OnConnectedAsync()
        {
            var username = Context.Connection.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return Clients.All.SendAsync("broadcastMessage", "_SYSTEM_", $"{username} JOINED");
        }

        // Uncomment this line to only allow user in Microsoft to send message
        // [Authorize(Policy = "Microsoft_Only")]
        public void broadcastMessage(string message)
        {
            var username = Context.Connection.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            Clients.All.SendAsync("broadcastMessage", username, message);
        }

        public void echo(string message)
        {
            var username = Context.Connection.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            Clients.Client(Context.ConnectionId).SendAsync("echo", username, message + " (echo from server)");
        }
    }
}
