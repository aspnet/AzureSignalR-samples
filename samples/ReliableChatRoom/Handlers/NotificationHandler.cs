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
        private readonly string _formatString = "{{ \"data\" : {{ \"sender\" : \"{0}\", \"text\" : \"{1}\" }} }}";

        public NotificationHandler(IUserHandler userHandler, string connectionString, string hubName)
        {
            _userHandler = userHandler;
            _notificationHub = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
        }

        public void SendBroadcastNotification(Message broadcastMessage)
        {
            Session senderSession = _userHandler.GetUserSession(broadcastMessage.Sender);
            if (senderSession != null)  //  Though sender session is very unlikely to be null
            {
                string jsonPayload = string.Format(_formatString, broadcastMessage.Sender, broadcastMessage.Text);
                string targetTagExpression = string.Format("! {0}", _userHandler.GetUserSession(broadcastMessage.Sender).DeviceUuid);
                Console.WriteLine(string.Format("SendBroadcastNotification to everyone but tag {0}\nJson: {1}", targetTagExpression, jsonPayload));
                _notificationHub.SendFcmNativeNotificationAsync(jsonPayload, targetTagExpression);
            }
        }

        public void SendPrivateNotification(Message privateMessage)
        {
            Session receiverSession = _userHandler.GetUserSession(privateMessage.Receiver);
            if (receiverSession != null) //  This happens when send to a non-existing receiver
            {
                string jsonPayload = string.Format(_formatString, privateMessage.Sender, privateMessage.Text);
                string targetTagExpression = string.Format("{0}", receiverSession.DeviceUuid);
                Console.WriteLine(string.Format("SendBroadcastNotification to tag {0}\nJson: {1}", targetTagExpression, jsonPayload));
                _notificationHub.SendFcmNativeNotificationAsync(jsonPayload, targetTagExpression);
            }            
        }
    }
}
