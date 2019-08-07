// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "ClaimBasedAuth")]
    [Authorize(Policy = "PolicyBasedAuth")]
    public class ChatRoomWithAck : Hub
    {
        private static readonly AckHandler AckHandler = new AckHandler();

        private readonly IMessageHandler _userMessage;

        private static readonly ConcurrentDictionary<string, string> UserList = new ConcurrentDictionary<string, string>();

        public ChatRoomWithAck(IMessageHandler userMessage)
        {
            _userMessage = userMessage;
        }

        public void Register(string name)
        {
            _userMessage.AddUser(name);
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + HubString.EchoNotification, name + message);
        }

        public async Task<string> SendUserMessage(string id, string sourceName, string targetName, string message)
        {
            var msg = new Message(id, sourceName, targetName, message, MessageType.UserTouser, DateTime.UtcNow);

            // Send the message to the target wait for target's ack
            var taskSend = AckHandler.CreateAckWithId(id);
            await Clients.User(targetName).SendAsync("sendUserMessage", id, sourceName, message, id);
            await taskSend;

            if (taskSend.Result.Equals(AckResult.TimeOut))
            {
                _userMessage.AddUnreadMessage(targetName, msg);
            }
            else if (taskSend.Result.Equals(AckResult.Success))
            {
                _userMessage.AddHistoryMessage(targetName, msg);
            }

            return taskSend.Result;
        }

        public async Task<string> SendUserAck(string msgId, string sourceName)
        {
            var (ackId, taskAck) = AckHandler.CreateAck();
            await Clients.User(sourceName).SendAsync("ackMessage", msgId, MessageStatus.Acknowledged, ackId);
            await taskAck;
            return taskAck.Result;
        }

        public string AckMessage(string ackId)
        {
            AckHandler.Ack(ackId);
            return AckResult.Success;
        }

        public async Task<string> LoadTempMessage(string sourceName)
        {
            if (_userMessage.IsUnreadEmpty(sourceName))
            {
                return LoadMessageResult.NoMessage;
            }

            while (!_userMessage.IsUnreadEmpty(sourceName))
            {
                var msg = _userMessage.PeekUnreadMessage(sourceName);
                await Clients.User(sourceName).SendAsync("sendUserMessage", msg.Id, msg.SourceName, msg.Text, AckResult.NoAck);
                _userMessage.PopUnreadMessage(sourceName);

                var (ackId, taskAck) = AckHandler.CreateAck();
                await Clients.User(msg.SourceName).SendAsync("ackMessage", msg.Id, MessageStatus.Arrived, ackId);
                await taskAck;
            }

            return LoadMessageResult.Success;
        }
    }
}
