﻿using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly ILogger _logger;

        // User handler for getting session info
        private readonly IUserHandler _userHandler;

        // Notification Hub Client for sending notification
        private readonly NotificationHubClient _notificationHubClient;
        
        // Format string for notification payload
        private readonly string _formatString = @"{{ ""data"" : {{ ""sender"" : ""{0}"", ""text"" : ""{1}"" }} }}";

        public NotificationHandler(
            ILogger<NotificationHandler> logger,
            IUserHandler userHandler,
            string connectionString,
            string hubName)
        {
            _logger = logger;
            _userHandler = userHandler;
            _notificationHubClient = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
        }

        public async Task SendBroadcastNotification(Message broadcastMessage)
        {
            Session senderSession = _userHandler.GetUserSession(broadcastMessage.Sender);
            if (senderSession != null)  //  Though sender session is very unlikely to be null
            {
                string jsonPayload = string.Format(_formatString, broadcastMessage.Sender, broadcastMessage.Payload);
                // TagExpression of "not USER_TAG", meaning sending to everyone but USER_TAG
                string targetTagExpression = string.Format("! {0}", _userHandler.GetUserSession(broadcastMessage.Sender).DeviceUuid);

                _logger.LogInformation("Send broadcast notification from {0}", broadcastMessage.Sender);
                await _notificationHubClient.SendFcmNativeNotificationAsync(jsonPayload, targetTagExpression);
            }
        }

        public async Task SendPrivateNotification(Message privateMessage)
        {
            Session receiverSession = _userHandler.GetUserSession(privateMessage.Receiver);
            if (receiverSession != null) //  Only happens when send to a non-existing receiver
            {
                string jsonPayload = string.Format(_formatString, privateMessage.Sender, privateMessage.Payload);
                string targetTagExpression = string.Format("{0}", receiverSession.DeviceUuid);

                _logger.LogInformation("Send private notification from {0} to {1}", privateMessage.Sender, privateMessage.Receiver);
                await _notificationHubClient.SendFcmNativeNotificationAsync(jsonPayload, targetTagExpression);
            }            
        }
    }
}
