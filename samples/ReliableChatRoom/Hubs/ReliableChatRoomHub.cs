using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs
{
    public class ReliableChatRoomHub : Hub
    {
        private readonly IClientAckHandler _clientAckHandler;
        private readonly IUserHandler _userHandler;
        private readonly IMessageStorage _messageStorage;
        private readonly CultureInfo _cultureInfo = new CultureInfo("en-US");
        private readonly int _maxRetryTimes = 3;
        private readonly Timer _resendTimer;
        IHubContext<ReliableChatRoomHub> _hubContext = null;

        public ReliableChatRoomHub(IHubContext<ReliableChatRoomHub> hubContext, IClientAckHandler clientAckHandler, IUserHandler userHandler, IMessageStorage messageStorage)
        {
            _hubContext = hubContext;
            _clientAckHandler = clientAckHandler;
            _userHandler = userHandler;
            _messageStorage = messageStorage;
            _resendTimer = new Timer(_ => ResendTimeOutMessages(), state: null, dueTime: TimeSpan.FromMilliseconds(500), period: TimeSpan.FromMilliseconds(2000));
        }

        public void EnterChatRoom(string deviceToken, string username)
        {
            Console.WriteLine(string.Format("EnterChatRoom device: {0} username: {1}", deviceToken, username));
            (string storedConnectionId, string storedDeviceToken) = _userHandler.Login(username, Context.ConnectionId, deviceToken);
            if (Context.ConnectionId.Equals(storedConnectionId) &&
                deviceToken.Equals(storedDeviceToken))
            {
                Message loginMessage = new Message(
                    Guid.NewGuid().ToString(),
                    Message.SYSTEM_SENDER,
                    Message.BROADCAST_RECEIVER,
                    string.Format("{0} has joined the chat", username),
                    MessageType.System,
                    DateTime.UtcNow);
                _messageStorage.TryStoreMessage(loginMessage, out bool _);
                Clients.All.SendAsync("broadcastSystemMessage", loginMessage.MessageId, loginMessage.Text);
            }
        }

        public void LeaveChatRoom(string deviceToken, string username)
        {
            Console.WriteLine(string.Format("LeaveChatRoom device: {0} username: {1}", deviceToken, username));
            _userHandler.Logout(username);
            Message logoutMessage = new Message(
                    Guid.NewGuid().ToString(),
                    Message.SYSTEM_SENDER,
                    Message.BROADCAST_RECEIVER,
                    string.Format("{0} has left the chat", username),
                    MessageType.System,
                    DateTime.UtcNow);
            _messageStorage.TryStoreMessage(logoutMessage, out bool _);
            Clients.All.SendAsync("broadcastSystemMessage", logoutMessage.MessageId, logoutMessage.Text);
        }

        public async void SendBroadcastMessage(string messageId, string sender, string payload)
        {
            Console.WriteLine(string.Format("SendBroadcastMessage {0} {1} {2}", messageId, sender, payload));
            //  Store message
            Message message = new Message(messageId, sender, Message.BROADCAST_RECEIVER, payload, MessageType.Broadcast, DateTime.UtcNow);
            _messageStorage.TryStoreMessage(message, out bool duplicatedMessageId);
            
            //  Sender server ack back to client
            await Clients.Client(Context.ConnectionId).SendAsync("serverAck", messageId);

            if (!duplicatedMessageId)
            {
                string senderConnectionId = Context.ConnectionId;
                var clientAck = _clientAckHandler.CreateClientAck(message);
                await Clients.AllExcept(Context.ConnectionId)
                    .SendAsync("displayBroadcastMessage",
                    message.MessageId,
                    message.Sender,
                    message.Receiver,
                    message.Text,
                    message.SendTime.ToString("MM/dd hh:mm:ss", _cultureInfo),
                    clientAck.ClientAckId);
            }
        }

        //  Complete the task specified by the ackId.
        public void ClientAckResponse(string clientAckId)
        {
            Console.WriteLine(string.Format("ClientAckResponse clientAckId: {0}", clientAckId));
            _clientAckHandler.Ack(clientAckId);
        }

        public void ClientReadResponse(string messageId, string username)
        {
            Clients.All.SendAsync("setMessageRead", messageId, username);
        }

        //  Send the message to the receiver
        public async void SendPrivateMessage(string messageId, string sender, string receiver, string payload)
        {

            Console.WriteLine(string.Format("SendPrivateMessage {0} {1} {2} {3}", messageId, sender, receiver, payload));
            //  Store message
            Message message = new Message(messageId, sender, receiver, payload, MessageType.Private, DateTime.UtcNow);
            _messageStorage.TryStoreMessage(message, out bool duplicatedMessageId);

            //  Sender server ack back to client
            await Clients.Client(Context.ConnectionId).SendAsync("serverAck", messageId);

            if (!duplicatedMessageId)
            {
                string senderConnectionId = Context.ConnectionId;
                string receiverConnectionId = _userHandler.GetUserConnectionId(receiver);
                if (receiverConnectionId != null)
                {
                    var clientAck = _clientAckHandler.CreateClientAck(message);
                    await Clients.AllExcept(Context.ConnectionId)
                        .SendAsync("displayPrivateMessage",
                        message.MessageId,
                        message.Sender,
                        message.Receiver,
                        message.Text,
                        message.SendTime.ToString("MM/dd hh:mm:ss", _cultureInfo),
                        clientAck.ClientAckId);
                }
                
            }
        }

        private void ResendTimeOutMessages()
        {
            var timeOutClientAcks =_clientAckHandler.GetTimeOutClientAcks();
            if (timeOutClientAcks.Count > 0)
            {
                Console.WriteLine(string.Format("{0} ack(s) messages resending", timeOutClientAcks.Count));
            }
            foreach (ClientAck clientAck in timeOutClientAcks)
            {
                if (clientAck.RetryCount < _maxRetryTimes)
                {
                    clientAck.Retry();
                    Message clientMessage = clientAck.ClientMessage;
                    IClientProxy clientProxy;
                    string retryMethod;
                    if (clientAck.ClientMessage.Type == MessageType.Broadcast)
                    {
                        string sender = clientMessage.Sender;
                        string senderConnectionId = _userHandler.GetUserConnectionId(sender);
                        clientProxy = _hubContext.Clients.AllExcept(senderConnectionId);
                        retryMethod = "displayBroadcastMessage";
                    } else if (clientAck.ClientMessage.Type == MessageType.Private)
                    {
                        string receiver = clientMessage.Receiver;
                        string receiverConnectionId = _userHandler.GetUserConnectionId(receiver);
                        clientProxy = _hubContext.Clients.Client(receiverConnectionId);
                        retryMethod = "displayPrivateMessage";
                    } else
                    {
                        continue;
                    }
                    Console.WriteLine("Retry messageId: {0}; sender: {1}; receiver: {2};", clientMessage.MessageId, clientMessage.Sender, clientMessage.Receiver);
                    clientProxy.SendAsync(retryMethod,
                                        clientMessage.MessageId,
                                        clientMessage.Sender,
                                        clientMessage.Receiver,
                                        clientMessage.Text,
                                        clientMessage.SendTime.ToString("MM/dd hh:mm:ss", _cultureInfo),
                                        clientAck.ClientAckId);
                } else
                {
                    clientAck.ClientAckResult = ClientAckResultEnum.Failure;
                }
            }
        }
    }
}
