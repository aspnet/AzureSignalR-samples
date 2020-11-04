using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class ClientAckHandler : IClientAckHandler, IDisposable
    {
        // HubContext used to send timed-out messages
        private readonly IHubContext<ReliableChatRoomHub> _hubContext;

        // UserHandler used to query user information
        private readonly IUserHandler _userHandler;

        /// In memory storage of <see cref="ClientAck"/> 
        private readonly ConcurrentDictionary<string, ClientAck> _clientAcks = new ConcurrentDictionary<string, ClientAck>();

        /// Max timespan a ClientAck can be Waiting without being called with <see cref="IClientAckHandler.Ack(string, string)"/>
        private readonly TimeSpan _checkAckThreshold;

        // Period of Timer checking the status of ClientAcks
        private readonly TimeSpan _checkAckInterval;

        // Max time to resend a un-acknowledged message
        private readonly int _resendMessageThreshold;

        // Period of Timer resending the timed-out messages 
        private readonly TimeSpan _resendMessageInterval;

        // Timers for checking ClientAcks and resending messages
        private readonly Timer _checkAckTimer;
        private readonly Timer _resendMessageTimer;

        // UNIX origin of time
        private readonly DateTime _javaEpoch = new DateTime(1970, 1, 1);

        public ClientAckHandler(IHubContext<ReliableChatRoomHub> hubContext, IUserHandler userHandler)
                    : this(
                           checkAckThreshold: TimeSpan.FromMilliseconds(5000),
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
            // Receivers involved in the ClientAck
            List<string> receivers;
            if (message.Type == MessageTypeEnum.Broadcast)
            {
                // Everyone except the sender
                receivers = new List<string>(_userHandler.GetActiveSessions().Select<Session, string>(sess => sess.Username));
                receivers.Remove(message.Sender);
            } else
            {
                // Only the receiver
                receivers = new List<string>() { message.Receiver };
            }

            ClientAck clientAck = new ClientAck(Guid.NewGuid().ToString(), DateTime.UtcNow, message, receivers);
            _clientAcks.TryAdd(clientAck.ClientAckId, clientAck);
            
            return clientAck;
        }

        public void Ack(string id, string username)
        {
            if (_clientAcks.TryGetValue(id, out var clientAck))
            {
                clientAck.Receivers.Remove(username);
            }
            else
            {
                Console.WriteLine("ClientAck id: {0} not found; sender: {1}.", id, username);
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
                    if (clientAck.Receivers.Count == 0)
                    {
                        clientAck.ClientAckResult = ClientAckResultEnum.Success;
                    } else
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
                    .SendAsync("receiveBroadcastMessage",
                                broadcastMessage.MessageId,
                                broadcastMessage.Sender,
                                broadcastMessage.Receiver,
                                broadcastMessage.Payload,
                                broadcastMessage.IsImage,
                                (broadcastMessage.SendTime - _javaEpoch).Ticks / TimeSpan.TicksPerMillisecond,
                                ackId);
        }

        private void ResendPrivateMessage(Message privateMessage, string ackId)
        {
            string receiverConnectionId = _userHandler.GetUserSession(privateMessage.Receiver).ConnectionId;
            Console.WriteLine(string.Format("ResendPrivateMessage: receiver connectionid: {0}", receiverConnectionId));
            _hubContext.Clients.Client(receiverConnectionId)
                    .SendAsync("receivePrivateMessage",
                                privateMessage.MessageId,
                                privateMessage.Sender,
                                privateMessage.Receiver,
                                privateMessage.Payload,
                                privateMessage.IsImage,
                                (privateMessage.SendTime - _javaEpoch).Ticks / TimeSpan.TicksPerMillisecond,
                                ackId);
        }
    }
}
