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
        private static ConcurrentDictionary<string, string> _connectedUsers = new ConcurrentDictionary<string, string>();

        public async Task Broadcast(string username, string message)
        {
            await Clients.All.SendAsync("Broadcast", username, message);
        }
        public async Task SendUserToUser(string senderUsername, string recipientUsername, string message)
        {
            var recipientConnectionId = _connectedUsers.FirstOrDefault(u => u.Value == recipientUsername).Key;
            if (!string.IsNullOrEmpty(recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId).SendAsync("SendUserToUser", senderUsername, recipientUsername, message);
            }
        }

        public override Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext().Request.Query["username"];
            _connectedUsers.TryAdd(Context.ConnectionId, username);
            Console.WriteLine($"{Context.ConnectionId}:${username} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            string username;
            _connectedUsers.TryRemove(Context.ConnectionId, out username);
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}:${username}");
            await base.OnDisconnectedAsync(e);
        }
        public IEnumerable<string> GetConnectedUsers()
        {
            return _connectedUsers.Values.Distinct();
        }
    }
}