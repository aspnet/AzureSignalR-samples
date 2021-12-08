// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR.Samples.AdvancedChatRoom
{
    public class ChatBase : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }

        public async Task JoinGroup(string name, string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("echo", "_SYSTEM_", $"{name} joined {groupName} with connectionId {Context.ConnectionId}");
        }

        public async Task LeaveGroup(string name, string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Client(Context.ConnectionId).SendAsync("echo", "_SYSTEM_", $"{name} leaved {groupName}");
            await Clients.Group(groupName).SendAsync("echo", "_SYSTEM_", $"{name} leaved {groupName}");
        }

        public void SendGroup(string name, string groupName, string message)
        {
            Clients.Group(groupName).SendAsync("echo", name, message);
        }

        public void SendGroups(string name, IReadOnlyList<string> groups, string message)
        {
            Clients.Groups(groups).SendAsync("echo", name, message);
        }

        public void SendGroupExcept(string name, string groupName, IReadOnlyList<string> connectionIdExcept, string message)
        {
            Clients.GroupExcept(groupName, connectionIdExcept).SendAsync("echo", name, message);
        }

        public void SendUser(string name, string userId, string message)
        {
            Clients.User(userId).SendAsync("echo", name, message);
        }

        public void SendUsers(string name, IReadOnlyList<string> userIds, string message)
        {
            Clients.Users(userIds).SendAsync("echo", name, message);
        }
    }
}
