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

        private readonly DateTime _javaEpoch = new DateTime(1970, 1, 1);

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
            _checkAckTimer = new Timer(_ => CheckAcks(), state: null, dueTime: TimeSpan.FromMilliseconds(0), period: _checkAckInterval);
            _resendMessageTimer = new Timer(_ => ResendTimeOutMessages(), state: null, dueTime: TimeSpan.FromMilliseconds(500), period: _resendMessageInterval);
        }

        public ClientAck CreateClientAck(Message message)
        {
            ClientAck clientAck = new ClientAck(Guid.NewGuid().ToString(), DateTime.UtcNow, message);
            _clientAcks.TryAdd(clientAck.ClientAckId, clientAck);
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
            foreach (ClientAck clientAck in _clientAcks.Values)
            {
                if (clientAck.ClientAckResult == ClientAckResultEnum.Waiting)
                {
                    var elapsed = DateTime.UtcNow - clientAck.ClientAckStartDateTime;
                    if (elapsed > _checkAckThreshold)
                    {
                        Console.WriteLine(string.Format("Ack id: {0} time out", clientAck.ClientAckId));
                        clientAck.TimeOut();
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
                //  No messages need to resend
                return;
            }


            foreach (ClientAck clientAck in timeOutClientAcks)
            {
                //  Only resend acks within threshold
                if (clientAck.RetryCount < _resendMessageThreshold)
                {
                    clientAck.Retry();
                    Console.WriteLine(string.Format("Retry {0}: {1}", clientAck.RetryCount, clientAck.ClientAckId));
                    Message clientMessage = clientAck.ClientMessage;
                    if (clientAck.ClientMessage.Type == MessageTypeEnum.Broadcast)
                    {
                        ResendBroadcastMessage(clientMessage, clientAck.ClientAckId);
                    }
                    else if (clientAck.ClientMessage.Type == MessageTypeEnum.Private)
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
            string senderConnectionId = _userHandler.GetUserSession(broadcastMessage.Sender).ConnectionId;
            Console.WriteLine(string.Format("ResendBroadcastMessage: sender connectionid: {0}", senderConnectionId));
            _hubContext.Clients.AllExcept(senderConnectionId)
                    .SendAsync("displayBroadcastMessage",
                                broadcastMessage.MessageId,
                                broadcastMessage.Sender,
                                broadcastMessage.Receiver,
                                broadcastMessage.Text,
                                (broadcastMessage.SendTime - _javaEpoch).Ticks / TimeSpan.TicksPerMillisecond,
                                ackId);
        }

        private void ResendPrivateMessage(Message privateMessage, string ackId)
        {
            string receiverConnectionId = _userHandler.GetUserSession(privateMessage.Receiver).ConnectionId;
            Console.WriteLine(string.Format("ResendPrivateMessage: receiver connectionid: {0}", receiverConnectionId));
            _hubContext.Clients.Client(receiverConnectionId)
                    .SendAsync("displayPrivateMessage",
                                privateMessage.MessageId,
                                privateMessage.Sender,
                                privateMessage.Receiver,
                                privateMessage.Text,
                                (privateMessage.SendTime - _javaEpoch).Ticks / TimeSpan.TicksPerMillisecond,
                                ackId);
        }
    }
}
