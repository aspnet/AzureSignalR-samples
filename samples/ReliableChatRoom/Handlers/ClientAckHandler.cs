using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class ClientAckHandler : IClientAckHandler, IDisposable
    {
        private readonly IHubContext<ReliableChatRoomHub> _hubContext;
        private readonly IUserHandler _userHandler;

        private readonly ConcurrentDictionary<string, ClientAck> _clientAcks = new ConcurrentDictionary<string, ClientAck>();

        private readonly TimeSpan _checkAckThreshold;
        private readonly TimeSpan _checkAckInterval;
        private readonly int _resendMessageThreshold;
        private readonly TimeSpan _resendMessageInterval;
        private readonly Timer _checkAckTimer;
        private readonly Timer _resendMessageTimer;

        public ClientAckHandler(IHubContext<ReliableChatRoomHub> hubContext, IUserHandler userHandler)
                    : this(
                           checkAckThreshold: TimeSpan.FromMilliseconds(10000),
                           checkAckInterval: TimeSpan.FromMilliseconds(500),
                           resendMessageThreshold: 3,
                           resendMessageInterval: TimeSpan.FromMilliseconds(1000))
        {
            _hubContext = hubContext;
            _userHandler = userHandler;
        }

        public ClientAckHandler(TimeSpan checkAckThreshold, TimeSpan checkAckInterval, int resendMessageThreshold, TimeSpan resendMessageInterval)
        {
            _checkAckThreshold = checkAckThreshold;
            _checkAckInterval = checkAckInterval;
            _resendMessageThreshold = resendMessageThreshold;
            _resendMessageInterval = resendMessageInterval;
            _checkAckTimer = new Timer(_ => CheckAcks(), state: null, dueTime: TimeSpan.FromSeconds(0), period: _checkAckInterval);
            _resendMessageTimer = new Timer(_ => ResendTimeOutMessages(), state: null, dueTime: TimeSpan.FromSeconds(500), period: _resendMessageInterval);
        }

        public ClientAck CreateClientAck(Message message)
        {
            string id = Guid.NewGuid().ToString();
            ClientAck clientAck = new ClientAck(id, DateTime.UtcNow, message);
            _clientAcks.TryAdd(id, clientAck);
            return clientAck;
        }

        public void Ack(string id)
        {
            if (_clientAcks.TryGetValue(id, out var clientAck))
            {
                clientAck.ClientAckResult = ClientAckResultEnum.Success;
            }
            else
            {
                throw new Exception("AckId not found");
            }
        }

        public void Dispose()
        {
            if (_checkAckTimer != null)
            {
                _checkAckTimer.Dispose();
                _resendMessageTimer.Dispose();
            }
        }

        private void CheckAcks()
        {
            foreach (var pair in _clientAcks)
            {
                string clientAckId = pair.Key;
                ClientAck clientAck = pair.Value;
                if (clientAck.ClientAckResult == ClientAckResultEnum.Waiting)
                {
                    var elapsed = DateTime.UtcNow - clientAck.ClientAckStartDateTime;
                    if (elapsed > _checkAckThreshold)
                    {
                        clientAck.ClientAckResult = ClientAckResultEnum.TimeOut;
                    }
                }
            }
        }

        private void ResendTimeOutMessages()
        {
            //  Calculate timeout acks
            var timeOutClientAcks = _clientAcks.Values.Where(ack => ack.ClientAckResult == ClientAckResultEnum.TimeOut).ToList();
            if (timeOutClientAcks.Count == 0)
            {
                Console.WriteLine("ResendTimeOutMessages: No messages need to resend");
                return;
            }


            foreach (ClientAck clientAck in timeOutClientAcks)
            {
                //  Only resend acks within threshold
                if (clientAck.RetryCount < _resendMessageThreshold)
                {
                    clientAck.Retry();
                    Message clientMessage = clientAck.ClientMessage;
                    if (clientAck.ClientMessage.Type == MessageType.Broadcast)
                    {
                        ResendBroadcastMessage(clientMessage, clientAck.ClientAckId);
                    }
                    else if (clientAck.ClientMessage.Type == MessageType.Private)
                    {
                        ResendPrivateMessage(clientMessage, clientAck.ClientAckId);
                    }
                }
                //  Acks being retried more than threshold are set to failure
                else
                {
                    clientAck.Fail();
                }
            }
        }

        private void ResendBroadcastMessage(Message broadcastMessage, string ackId)
        {
            string senderConnectionId = _userHandler.GetUserConnectionId(broadcastMessage.Sender);
            
            _hubContext.Clients.AllExcept(senderConnectionId).SendAsync(
                "displayBroadcastMessage",
                broadcastMessage.MessageId,
                broadcastMessage.Sender,
                broadcastMessage.Receiver,
                broadcastMessage.Text,
                broadcastMessage.SendTime.ToString("MM/dd hh:mm:ss"),
                ackId);
        }

        private void ResendPrivateMessage(Message privateMessage, string ackId)
        {
            string receiverConnectionId = _userHandler.GetUserConnectionId(privateMessage.Receiver);

            _hubContext.Clients.Client(receiverConnectionId).SendAsync(
                "displayPrivateMessage",
                privateMessage.MessageId,
                privateMessage.Sender,
                privateMessage.Receiver,
                privateMessage.Text,
                privateMessage.SendTime.ToString("MM/dd hh:mm:ss"),
                ackId);
        }
    }
}
