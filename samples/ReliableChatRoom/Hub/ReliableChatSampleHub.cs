// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class ReliableChatSampleHub : Hub
    {
        private readonly IMessageHandler _messageHandler;

        private readonly ISessionHandler _sessionHandler;

        public ReliableChatSampleHub(IMessageHandler messageHandler, ISessionHandler sessionHandler)
        {
            _messageHandler = messageHandler;
            _sessionHandler = sessionHandler;
        }

        public override async Task OnConnectedAsync()
        {
            var sender = Context.UserIdentifier;

            //  Push the latest session information to the user.
            //  Currently push the whole list. Better pushing the timestamp to make incremental updates. 
            var userSessions = await _sessionHandler.GetAllSessionsAsync(sender);
            await Clients.Caller.SendAsync("updateSessions", userSessions);
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Broadcast a message to all clients. Won't store the message.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="messageContent"></param>
        public async Task<int> BroadcastMessage(string userName, string messageContent)
        {
            var message = new Message(userName, DateTime.Now, messageContent, "Sent");
            var sequenceId = await _messageHandler.AddNewMessageAsync("Public", message);

            if(userName=="Public" || userName == "_SYSTEM_")
            {
                await Clients.All.SendAsync("displayUserMessage", "Public", sequenceId, userName, messageContent);
            } else
            {
                await Clients.Others.SendAsync("displayUserMessage", "Public", sequenceId, userName, messageContent);
            }

            return sequenceId;
        }

        /// <summary>
        /// Create a new session with a specified user.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>The sessionId.</returns>
        public async Task<string> GetOrCreateNewSession(string receiver)
        {
            var sender = Context.UserIdentifier;
            var session = await _sessionHandler.GetOrCreateSessionAsync(sender, receiver);

            return session.SessionId;
        }

        /// <summary>
        /// Send a message to the specified user.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="receiver"></param>
        /// <param name="messageContent"></param>
        /// <returns>The sequenceId of the message.</returns>
        public async Task<int> SendUserMessage(string sessionId, string receiver, string messageContent)
        {
            var sender = Context.UserIdentifier;

            if(sessionId == "Public")
            {
                return await BroadcastMessage(sender, messageContent);
            }

            var message = new Message(sender, DateTime.Now, messageContent, "Sent");
            var sequenceId = await _messageHandler.AddNewMessageAsync(sessionId, message);

            await Clients.User(receiver).SendAsync("displayUserMessage", sessionId, sequenceId, sender, messageContent);

            return sequenceId;
        }

        /// <summary>
        /// Send an ack to the message owner.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="sequenceId"></param>
        /// <param name="receiver"></param>
        /// <param name="messageStatus"></param>
        /// <returns>The status of the message.</returns>
        public async Task<string> SendUserResponse(string sessionId, int sequenceId, string receiver, string messageStatus)
        {
            await _messageHandler.UpdateMessageAsync(sessionId, sequenceId, messageStatus);

            await Clients.User(receiver).SendAsync("displayResponseMessage", sessionId, sequenceId, messageStatus);

            return messageStatus;
        }

        /// <summary>
        /// Load the unread/history messages of one session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<List<Message>> LoadMessages(string sessionId)
        {
            return await _messageHandler.LoadHistoryMessageAsync(sessionId, -1, -1);
        }
    }
}
