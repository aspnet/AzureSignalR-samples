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
            var userSessions = await _sessionHandler.GetLatestSessionsAsync(sender);

            //  Send to latest session list to user.
            await Clients.Caller.SendAsync("updateSessions", userSessions);

            var onConnectedMessage = sender + " joined the chat room";
            var message = new Message("Public", DateTime.Now, onConnectedMessage, "Sent");
            var sequenceId = await _messageHandler.AddNewMessageAsync("Public", message);
            await Clients.All.SendAsync("displayUserMessage", "Public", sequenceId, "Public", onConnectedMessage);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var sender = Context.UserIdentifier;

            var onDisconnectedMessage = sender + " left the chat room";
            var message = new Message("Public", DateTime.Now, onDisconnectedMessage, "Sent");
            var sequenceId = await _messageHandler.AddNewMessageAsync("Public", message);
            await Clients.All.SendAsync("displayUserMessage", "Public", sequenceId, "Public", onDisconnectedMessage);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Broadcast a message to all clients.
        /// </summary>
        /// <param name="messageContent"></param>
        public async Task<string> BroadcastMessage(string messageContent)
        {
            var sender = Context.UserIdentifier;
            var message = new Message(sender, DateTime.Now, messageContent, "Sent");
            var sequenceId = await _messageHandler.AddNewMessageAsync("Public", message);

            await Clients.Others.SendAsync("displayUserMessage", "Public", sequenceId, sender, messageContent);

            return sequenceId;
        }

        /// <summary>
        /// Create a new session with a specified user.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>The sessionId.</returns>
        public async Task<string> GetOrCreateSession(string receiver)
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
        public async Task<string> SendUserMessage(string sessionId, string receiver, string messageContent)
        {
            var sender = Context.UserIdentifier;

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
        public async Task<string> SendUserResponse(string sessionId, string sequenceId, string receiver, string messageStatus)
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
            return await _messageHandler.LoadHistoryMessageAsync(sessionId);
        }
    }
}
