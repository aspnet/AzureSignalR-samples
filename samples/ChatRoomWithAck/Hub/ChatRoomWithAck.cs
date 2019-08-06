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
        private static readonly AckHandler AckHandler = new AckHandler();

        private static readonly IMessageHandler UserMessage = new Assembler().Create("StaticMessageStorage");

        private static readonly ConcurrentDictionary<string, string> UserList = new ConcurrentDictionary<string, string>();

        public void Register(string name)
        {
            if (UserList.ContainsKey(name))
            {
                UserList.TryRemove(name, out _);
                UserList.TryAdd(name, Context.ConnectionId);
                return;
            }

            UserList.TryAdd(name, Context.ConnectionId);
            UserMessage.AddUser(name);
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
            if (GetConnectionId(targetName).Length == 0)
            {
                throw new NullReferenceException();
            }

            var msg = new Message(id, sourceName, targetName, message, MessageType.UserTouser, DateTime.UtcNow);

            // Send the message to the target wait for target's ack
            var taskSend = AckHandler.CreateAckWithId(id);
            await Clients.Client(GetConnectionId(targetName)).SendAsync("sendUserMessage", id, sourceName, message, id);
            await taskSend;

            if (taskSend.Result.Equals(AckResult.TimeOut))
            {
                UserMessage.AddUnreadMessage(targetName, msg);
            }
            else if (taskSend.Result.Equals(AckResult.Success))
            {
               UserMessage.AddHistoryMessage(targetName, msg);
            }

            return taskSend.Result;
        }

        public async Task<string> SendUserAck(string msgId, string sourceName)
        {
            var (ackId, taskAck) = AckHandler.CreateAck();
            await Clients.Client(GetConnectionId(sourceName)).SendAsync("ackMessage", msgId, MessageStatus.Acknowledged, ackId);
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
            if(UserMessage.IsUnreadEmpty(sourceName))
            {
                return LoadMessageResult.NoMessage;
            }

            while (UserMessage.IsUnreadEmpty(sourceName))
            {
                var msg = UserMessage.PeekUnreadMessage(sourceName);
                await Clients.Client(GetConnectionId(sourceName)).SendAsync("sendUserMessage", msg.Id, msg.SourceName, msg.Text, AckResult.NoAck);
                UserMessage.PopUnreadMessage(sourceName);

                var (ackId, taskAck) = AckHandler.CreateAck();
                await Clients.Client(GetConnectionId(msg.SourceName)).SendAsync("ackMessage", msg.Id, MessageStatus.Arrived, ackId);
                await taskAck;
            }

            return LoadMessageResult.Success;
        }
        
        private static string GetConnectionId(string name)
        {
            return UserList.TryGetValue(name, out var id) ? id : "";
        }
    }
}
