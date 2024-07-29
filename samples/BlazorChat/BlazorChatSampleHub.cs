using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

namespace BlazorChat
{
    public class BlazorChatSampleHub : Hub
    {
        public const string HubUrl = "/chat";
        // cache with <UserName,ConnectionIds>
        private static ConcurrentDictionary<string, List<string>> _connectedUsers = new ConcurrentDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public async Task Broadcast(string username, string recipient, string message)
        {
            if (string.IsNullOrEmpty(recipient))
            {
                await Clients.All.SendAsync("Broadcast", username, message);
            }
            else if (_connectedUsers.TryGetValue(recipient, out var connections) && connections.Count > 0)
            {
                await Clients.Clients(connections).SendAsync("SendToUser", username, recipient, message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext().Request.Query["username"];
            if (_connectedUsers.TryGetValue(username, out var connections))
            {
                connections.Add(Context.ConnectionId);
            }
            else
            {
                connections = new List<string>() { Context.ConnectionId };
            }
            _connectedUsers[username] = connections;
            Console.WriteLine($"{Context.ConnectionId}:${username} connected");

            await Clients.All.SendAsync("UpdateConnectedUsers", _connectedUsers.Keys);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            var username = Context.GetHttpContext().Request.Query["username"];
            if (_connectedUsers.TryGetValue(username, out var connections) && connections.Contains(Context.ConnectionId))
            {
                connections.Remove(Context.ConnectionId);
                _connectedUsers[username] = connections;
                if (connections.Count == 0)
                {
                    _connectedUsers.Remove(username, out var removed);
                }
            }
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}:${username}");

            await Clients.All.SendAsync("UpdateConnectedUsers", _connectedUsers.Keys);
            await base.OnDisconnectedAsync(e);
        }
    }
}