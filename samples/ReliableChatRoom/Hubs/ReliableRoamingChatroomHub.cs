// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs
{
    public class ReliableRoamingChatroomHub : ReliableChatRoomHub
    {
        private readonly IMessageHandler _userMessage;

        public ReliableRoamingChatroomHub(IAckHandler ackHandler, ILoginHandler loginHandler, IMessageHandler userMessage) : base(ackHandler, loginHandler)
        {
            _userMessage = userMessage;
        }

        //  Store the message in the history/unread list after sending the message.
        public async Task<string> SendUserRoamingMessage(string messageId, string sender, string receiver, string time, string message)
        {
            var msg = new Message(messageId, sender, receiver, message, MessageType.UserToUser, DateTime.UtcNow);

            var result = await SendUserMessage(messageId, sender, receiver, time, message);

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
