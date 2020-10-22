using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs
{
    public class ReliableChatRoomHub : Hub
    {
        private readonly IUserHandler _userHandler;
        private readonly IMessageStorage _messageStorage;
        private readonly IMessageFactory _messageFactory;
        private readonly IClientAckHandler _clientAckHandler;
        private readonly INotificationHandler _notificationHandler;

        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);


        public ReliableChatRoomHub(
            IUserHandler userHandler,
            IMessageStorage messageStorage,
            IMessageFactory messageFactory,
            IClientAckHandler clientAckHandler,
            INotificationHandler notificationHandler)
        {
            _userHandler = userHandler;
            _messageStorage = messageStorage;
            _messageFactory = messageFactory;
            _clientAckHandler = clientAckHandler;
            _notificationHandler = notificationHandler;
        }


        //  User periodically touches server to extend his session
        public void TouchServer(string deviceUuid, string username)
        {
            DateTime touchedDateTime = _userHandler.Touch(username, Context.ConnectionId, deviceUuid);
            if (touchedDateTime == _defaultDateTime) //  Session either does not exist or expires
            {
                Clients.Caller.SendAsync("expireSession", true);
            }
        }

        public void EnterChatRoom(string deviceUuid, string username)
        {
            Console.WriteLine(string.Format("EnterChatRoom device: {0} username: {1}", deviceUuid, username));
            
            //  Try to store user login information (ConnectionId & deviceUuid)
            Session session = _userHandler.Login(username, Context.ConnectionId, deviceUuid);
            
            //  If login was successful, broadcast the system message 
            if (session != null)
            {
                Message loginMessage = _messageFactory.CreateSystemMessage(username, "joined", DateTime.UtcNow);
                //  Do not store system messages. Directly send them out.
                SendSystemMessage(loginMessage);
            }
        }

        public void LeaveChatRoom(string connectionId)
        {
            Console.WriteLine(string.Format("LeaveChatRoom connectionId: {0}", connectionId));

            //  Do not care about logout result
            Session session = _userHandler.Logout(connectionId);

            //  Broadcast the system message
            Message logoutMessage = _messageFactory.CreateSystemMessage(session.Username, "left", DateTime.UtcNow);
            //  Do not store system messages. Directly send them out.
            SendSystemMessage(logoutMessage);
        }

        public void OnBroadcastMessageReceived(string messageId, string sender, string payload)
        {
            Console.WriteLine(string.Format("OnBroadcastMessageReceived {0} {1} {2}", messageId, sender, payload));

            //  Create and store message
            Message message = _messageFactory.CreateBroadcastMessage(messageId, sender, payload, DateTime.UtcNow);
            bool isStored = _messageStorage.TryStoreMessage(message);

            //  Send back a server ack regardless of whether is a duplicated message
            long receivedTimeInLong = CSharpDateTimeToJavaLong(message.SendTime);
            Clients.Client(Context.ConnectionId).SendAsync("serverAck", message.MessageId, receivedTimeInLong);

            if (isStored)
            {
                SendBroadCastMessage(message);
                _notificationHandler.SendBroadcastNotification(message);
            }
        }

        public void OnPrivateMessageReceived(string messageId, string sender, string receiver, string payload)
        {

            Console.WriteLine(string.Format("OnPrivateMessageReceive {0} {1} {2} {3}", messageId, sender, receiver, payload));
            
            //  Create and store message
            Message message = _messageFactory.CreatePrivateMessage(messageId, sender, receiver, payload, DateTime.UtcNow);
            bool isStored = _messageStorage.TryStoreMessage(message);

            //  Sender server ack back to client
            long receivedTimeInLong = CSharpDateTimeToJavaLong(message.SendTime);
            Clients.Client(Context.ConnectionId).SendAsync("serverAck", message.MessageId, receivedTimeInLong);

            if (isStored)
            {
                SendPrivateMessage(message);
                _notificationHandler.SendPrivateNotification(message);
            }
        }

        public void OnAckResponseReceived(string clientAckId)
        {
            Console.WriteLine(string.Format("OnAckResponseReceived clientAckId: {0}", clientAckId));
            
             //  Complete the waiting client ack object
            _clientAckHandler.Ack(clientAckId);
        }

        public void OnReadResponseReceived(string messageId, string username)
        {
            Console.WriteLine(string.Format("OnReadResponseReceived messageId: {0}; username: {1}", messageId, username));
            
            //  Broadcast message read by user
            Clients.All.SendAsync("setMessageRead", messageId, username);
        }

        public void OnPullHistoryMessagesReceived(string username, long untilTime)
        {
            Console.WriteLine(string.Format("OnPullHistoryMessageReceived username: {0}; until: {1}", username, untilTime));

            var untilDateTime = JavaLongToCSharpDateTime(untilTime);

            List<Message> historyMessages = _messageStorage.GetHistoryMessage(username, untilDateTime);
            Clients.Client(Context.ConnectionId).SendAsync("addHistoryMessages", JsonConvert.SerializeObject(historyMessages));
        }

        private void SendSystemMessage(Message systemMessage)
        {
            Clients.All.SendAsync("broadcastSystemMessage",
                systemMessage.MessageId,
                systemMessage.Text,
                CSharpDateTimeToJavaLong(systemMessage.SendTime));
        }

        private void SendBroadCastMessage(Message broadcastMessage)
        {
            //  Create a client ack 
            var clientAck = _clientAckHandler.CreateClientAck(broadcastMessage);

            //  Broadcast to all other users
            Clients.AllExcept(Context.ConnectionId)
                    .SendAsync("displayBroadcastMessage",
                                broadcastMessage.MessageId,
                                broadcastMessage.Sender,
                                broadcastMessage.Receiver,
                                broadcastMessage.Text,
                                CSharpDateTimeToJavaLong(broadcastMessage.SendTime),
                                clientAck.ClientAckId);
        }

        private void SendPrivateMessage(Message privateMessage)
        {
            //  Create a client ack 
            var clientAck = _clientAckHandler.CreateClientAck(privateMessage);

            //  Send only to receiver
            Clients.Client(_userHandler.GetUserSession(privateMessage.Receiver).ConnectionId)
                    .SendAsync("displayPrivateMessage",
                                privateMessage.MessageId,
                                privateMessage.Sender,
                                privateMessage.Receiver,
                                privateMessage.Text,
                                CSharpDateTimeToJavaLong(privateMessage.SendTime),
                                clientAck.ClientAckId);
        }

        private DateTime JavaLongToCSharpDateTime(long milliseconds)
        {
            long ticks = milliseconds * TimeSpan.TicksPerMillisecond + _defaultDateTime.Ticks;
            return new DateTime(ticks);
        }

        private long CSharpDateTimeToJavaLong(DateTime dateTime)
        {
            return (dateTime - _defaultDateTime).Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
