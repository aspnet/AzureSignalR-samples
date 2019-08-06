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
        private static AckHandler _ackhandler = new AckHandler();

        private static IMessageHandler _userMessage = new Assembler().Create("StaticMessageStorage");

        private static ConcurrentDictionary<string, string> _userList = new ConcurrentDictionary<string, string>();

        public void Register(string name)
        {
            if (_userList.ContainsKey(name))
            {
                _userList.TryRemove(name, out string val);
                _userList.TryAdd(name, Context.ConnectionId);
                return;
            }

            _userList.TryAdd(name, Context.ConnectionId);
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
            if (GetConnectionId(targetName).Length == 0)
            {
                throw new NullReferenceException();
            }

            Message _msg = new Message(id, sourceName, targetName, message, MessageType.UserTouser, DateTime.UtcNow);

            // Send the message to the target wait for target's ack
            var _taskSend = _ackhandler.CreateAckWithId(id);
            await Clients.Client(GetConnectionId(targetName)).SendAsync("sendUserMessage", id, sourceName, message, id);
            await _taskSend;

            if (_taskSend.Result.Equals(AckResult.TimeOut))
            {
                _userMessage.AddUnreadMessage(targetName, _msg);
            }
            else if (_taskSend.Result.Equals(AckResult.Success))
            {
               _userMessage.AddHistoryMessage(targetName, _msg);
            }

            return _taskSend.Result;
        }

        public async Task<string> SendUserAck(string msgId, string sourceName)
        {
            var (ackId, taskAck) = _ackhandler.CreateAck();
            await Clients.Client(GetConnectionId(sourceName)).SendAsync("ackMessage", msgId, MessageStatus.Acknowledged, ackId);
            await taskAck;
            return taskAck.Result;
        }

        public string AckMessage(string ackId)
        {
            _ackhandler.Ack(ackId);
            return AckResult.Success;
        }

        public async Task<string> LoadTempMessage(string sourceName)
        {
            if(_userMessage.IsUnreadEmpty(sourceName))
            {
                return LoadMessageResult.NoMessage;
            }

            while (_userMessage.IsUnreadEmpty(sourceName))
            {
                Message msg = _userMessage.PeekUnreadMessage(sourceName);
                await Clients.Client(GetConnectionId(sourceName)).SendAsync("sendUserMessage", msg.Id, msg.SourceName, msg.Text, AckResult.NoAck);
                _userMessage.PopUnreadMessage(sourceName);

                var (ackId, taskAck) = _ackhandler.CreateAck();
                await Clients.Client(GetConnectionId(msg.SourceName)).SendAsync("ackMessage", msg.Id, MessageStatus.Arrived, ackId);
                await taskAck;
            }

            return LoadMessageResult.Success;
        }
        
        private static string GetConnectionId(string name)
        {
            if (_userList.TryGetValue(name, out string id))
            {
                return id;
            }
            else return "";
        }
    }
}
