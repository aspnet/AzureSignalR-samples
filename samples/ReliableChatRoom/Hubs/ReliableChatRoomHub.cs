using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage;
using System;
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
        public void TouchServer(string registrationId, string username)
        {
            DateTime touchedDateTime = _userHandler.Touch(username, Context.ConnectionId, registrationId);
            if (touchedDateTime == _defaultDateTime) //  Session either does not exist or expires
            {
                Clients.Caller.SendAsync("expireSession");
            }
        }

        public void EnterChatRoom(string registrationId, string username)
        {
            Console.WriteLine(string.Format("EnterChatRoom device: {0} username: {1}", registrationId, username));
            
            //  Try to store user login information (ConnectionId & registrationId)
            Session session = _userHandler.Login(username, Context.ConnectionId, registrationId);
            
            //  If login was successful, broadcast the system message 
            if (session != null)
            {
                Message loginMessage = _messageFactory.CreateSystemMessage(username, "joined", DateTime.UtcNow);
                _messageStorage.TryStoreMessage(loginMessage);
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
            _messageStorage.TryStoreMessage(logoutMessage);
            SendSystemMessage(logoutMessage);
        }

        public void OnBroadcastMessageReceived(string messageId, string sender, string payload)
        {
            Console.WriteLine(string.Format("OnBroadcastMessageReceived {0} {1} {2}", messageId, sender, payload));

            //  Create and store message
            Message message = _messageFactory.CreateBroadcastMessage(messageId, sender, payload, DateTime.UtcNow);
            bool isStored = _messageStorage.TryStoreMessage(message);

            //  Send back a server ack regardless of whether is a duplicated message
            Clients.Client(Context.ConnectionId).SendAsync("serverAck", message.MessageId);

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
            Clients.Client(Context.ConnectionId).SendAsync("serverAck", message.MessageId);

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

        private void SendSystemMessage(Message systemMessage)
        {
            Clients.All.SendAsync("broadcastSystemMessage", systemMessage.MessageId, systemMessage.Text);
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
                                (broadcastMessage.SendTime - _defaultDateTime).Ticks / TimeSpan.TicksPerMillisecond,
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
                                (privateMessage.SendTime - _defaultDateTime).Ticks / TimeSpan.TicksPerMillisecond,
                                clientAck.ClientAckId);
        }
    }
}
