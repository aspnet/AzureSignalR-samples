using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly IUserHandler _userHandler;
        private readonly NotificationHubClient _notificationHub;

        public NotificationHandler(IUserHandler userHandler, string hubName, string connectionString)
        {
            _userHandler = userHandler;
            _notificationHub = new NotificationHubClient(connectionString, hubName);
        }

        public void SendBroadcastNotification(Message broadcastMessage)
        {
            string formatString = "{ \"data\" : { \"sender\" : \"{0}\", \"text\" : {1}}}";
            string jsonPayload = string.Format(formatString, broadcastMessage.Sender, broadcastMessage.Text);
            _notificationHub.SendFcmNativeNotificationAsync(jsonPayload);
        }

        public void SendPrivateNotification(Message privateMessage)
        {
            throw new NotImplementedException();
        }
    }
}
