// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class ChatRoomWithAck : Hub
    {
        private readonly IAckHandler _ackHandler;

        private readonly IMessageHandler _userMessage;

        private static readonly ConcurrentDictionary<string, string> UserList = new ConcurrentDictionary<string, string>();

        public ChatRoomWithAck(IMessageHandler userMessage, IAckHandler ackHandler)
        {
            _ackHandler = ackHandler;
            _userMessage = userMessage;
        }

        public void Register(string name)
        {
            if (UserList.ContainsKey(name))
            {
                UserList.TryRemove(name, out _);
                UserList.TryAdd(name, Context.ConnectionId);
                return;
            }

            UserList.TryAdd(name, Context.ConnectionId);
            _userMessage.AddUser(name);
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public async Task<string> SendUserMessage(string id, string sourceName, string targetName, string message)
        {
            if (GetConnectionId(targetName).Length == 0)
            {
                throw new NullReferenceException();
            }

            var msg = new Message(id, sourceName, targetName, message, MessageType.UserToUser, DateTime.UtcNow);

            // Send the message to the target wait for target's ack
            var ackInfo = _ackHandler.CreateAck();
            await Clients.Client(GetConnectionId(targetName)).SendAsync("sendUserMessage", id, sourceName, message);
            var result = await ackInfo.AckTask;

            if (result.Equals(AckResult.TimeOut))
            {
                _userMessage.AddUnreadMessage(targetName, msg);
            }
            else if (result.Equals(AckResult.Success))
            {
                _userMessage.AddHistoryMessage(targetName, msg);
                return MessageStatus.Arrived.ToString();
            }

            return result.ToString();
        }

        public async Task<string> SendUserAck(string msgId, string sourceName)
        {
            var ackInfo = _ackHandler.CreateAck();
            await Clients.Client(GetConnectionId(sourceName))
                .SendAsync("ackMessage", msgId, MessageStatus.Acknowledged.ToString(), ackInfo.AckId);
            return (await ackInfo.AckTask).ToString();
        }

        public void AckMessage(string ackId)
        {
            _ackHandler.Ack(ackId);
        }

        public async Task<string> LoadUnreadMessage(string sourceName)
        {
            if (_userMessage.IsUnreadEmpty(sourceName))
            {
                return LoadMessageResult.NoMessage;
            }

            while (!_userMessage.IsUnreadEmpty(sourceName))
            {
                var msg = _userMessage.PeekUnreadMessage(sourceName);
                await Clients.Client(GetConnectionId(sourceName))
                    .SendAsync("sendUserMessage", msg.Id, msg.SourceName, msg.Text, AckResult.NoAck);
                _userMessage.PopUnreadMessage(sourceName);

                var ackInfo = _ackHandler.CreateAck();
                await Clients.Client(GetConnectionId(msg.SourceName))
                    .SendAsync("ackMessage", msg.Id, MessageStatus.Arrived.ToString(), ackInfo.AckId);
                await ackInfo.AckTask;
            }

            return LoadMessageResult.Success;
        }

        private static string GetConnectionId(string name)
        {
            return UserList.TryGetValue(name, out var id) ? id : "";
        }
    }
}
