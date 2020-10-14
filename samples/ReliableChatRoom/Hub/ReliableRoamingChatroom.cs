// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class ReliableRoamingChatRoom : ReliableChatRoom
    {
        private readonly IMessageHandler _userMessage;

        public ReliableRoamingChatRoom(IAckHandler ackHandler, IMessageHandler userMessage) : base(ackHandler)
        {
            _userMessage = userMessage;
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        //  Store the message in the history/unread list after sending the message.
        public async Task<string> SendUserRoamingMessage(string messageId, string sender, string receiver, string message)
        {
            var msg = new Message(messageId, sender, receiver, message, MessageType.UserToUser, DateTime.UtcNow);

            var result = await SendUserMessage(messageId, sender, receiver, message);

            if (result.Equals(AckResult.TimeOut.ToString()))
            {
                _userMessage.AddUnreadMessage(receiver, msg);
            }
            else if (result.Equals(AckResult.Success.ToString()))
            {
                _userMessage.AddHistoryMessage(receiver, msg);
                return MessageStatus.Arrived.ToString();
            }

            return result;
        }

        //  Pull the unread message when the client connects. 
        public async Task<string> LoadUnreadMessage(string sourceName)
        {
            if (_userMessage.IsUnreadEmpty(sourceName))
            {
                return LoadMessageResult.NoMessage;
            }

            while (!_userMessage.IsUnreadEmpty(sourceName))
            {
                var msg = _userMessage.PeekUnreadMessage(sourceName);
                await Clients.User(sourceName)
                    .SendAsync("displayUserMessage", msg.Id, msg.SourceName, msg.Text, AckResult.NoAck);
                _userMessage.PopUnreadMessage(sourceName);
            }

            return LoadMessageResult.Success;
        }
    }
}
