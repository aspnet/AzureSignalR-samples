// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ChatSampleHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return Clients.All.SendAsync("broadcastMessage", "_SYSTEM_", $"{Context.User?.Identity?.Name} JOINED");
    }

    // Uncomment this line to only allow user in Microsoft to send message
    //[Authorize(Policy = "Microsoft_Only")]
    public Task BroadcastMessage(string message)
    {
        return Clients.All.SendAsync("broadcastMessage", Context.User?.Identity?.Name, message);
    }

    public Task Echo(string message)
    {
        var echoMessage = $"{message} (echo from server)";
        return Clients.Client(Context.ConnectionId).SendAsync("echo", Context.User?.Identity?.Name, echoMessage);
    }
}