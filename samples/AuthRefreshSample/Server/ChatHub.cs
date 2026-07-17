// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AuthRefreshSample;

[Authorize]
public sealed class ChatHub : Hub
{
    public override Task OnConnectedAsync() =>
        Clients.Caller.SendAsync("ReceiveMessage", "system", $"connected as {Context.UserIdentifier}");

    public Task Broadcast(string message) =>
        Clients.All.SendAsync("ReceiveMessage", Context.UserIdentifier ?? "anonymous", message);

    // Runs after Azure SignalR applies the refreshed claims to Context.User. React to a refresh here.
    public override Task OnAuthenticationRefreshedAsync() =>
        Clients.Caller.SendAsync("ReceiveMessage", "system", $"auth refreshed for {Context.UserIdentifier}");
}
