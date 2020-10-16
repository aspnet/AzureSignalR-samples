using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class ClientAckHandler : IClientAckHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, ClientAck> _clientAcks
            = new ConcurrentDictionary<string, ClientAck>();

        private readonly TimeSpan _ackThreshold;

        private readonly int _ackRetryThreshold;

        private Timer _timer;

        private Timer _retryTimer;

        private Hub _hub;

        public ClientAckHandler()
                    : this(completeAcksOnTimeout: true,
                           ackThreshold: TimeSpan.FromSeconds(10),
                           ackInterval: TimeSpan.FromSeconds(0.5),
                           ackRetryThreshold: 3)
        {
        }

        public ClientAckHandler(bool completeAcksOnTimeout, TimeSpan ackThreshold, TimeSpan ackInterval, int ackRetryThreshold)
        {
            if (completeAcksOnTimeout)
            {
                _timer = new Timer(_ => CheckAcks(), state: null, dueTime: TimeSpan.FromSeconds(0), period: ackInterval);
            }

            _retryTimer = new Timer(_ => RetryAcks(), state: null, dueTime: TimeSpan.FromSeconds(0.25), period: ackInterval);

            _ackThreshold = ackThreshold;
            _ackRetryThreshold = ackRetryThreshold;
        }

        public void SetHub(Hub hub)
        {
            this._hub = hub;
        }

        public ClientAck CreateClientAck(Message message, string retryMethod, string senderConnectionId, string receiverConnectionId)
        {
            string id = Guid.NewGuid().ToString();
            ClientAck clientAck = new ClientAck(id, DateTime.UtcNow, message, retryMethod, senderConnectionId, receiverConnectionId);
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
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        private void CheckAcks()
        {
            foreach (var pair in _clientAcks)
            {
                string clientAckId = pair.Key;
                ClientAck clientAck = pair.Value;
                var elapsed = DateTime.UtcNow - clientAck.ClientAckStartDateTime;
                if (elapsed > _ackThreshold)
                {
                    clientAck.ClientAckResult = ClientAckResultEnum.TimeOut;
                }
            }
        }

        private async void RetryAcks()
        {
            foreach (var pair in _clientAcks)
            {
                string clientAckId = pair.Key;
                ClientAck clientAck = pair.Value;
                if (clientAck.ClientAckResult == ClientAckResultEnum.TimeOut &&
                    clientAck.RetryCount < _ackRetryThreshold)
                {
                    // Resend message and wait for ack
                    clientAck.ClientAckResult = ClientAckResultEnum.Waiting;
                    clientAck.IncRetryCount();
                    if (clientAck.ClientMessage.Type == MessageType.Broadcast)
                    {
                        await _hub.Clients.AllExcept(clientAck.SenderConnectionId)
                            .SendAsync(clientAck.RetryMethod,
                            clientAck.ClientMessage.MessageId,
                            clientAck.ClientMessage.Sender,
                            clientAck.ClientMessage.Receiver,
                            clientAck.ClientMessage.Text,
                            clientAck.ClientMessage.SendTime,
                            clientAck.ClientAckId);
                    } else if (clientAck.ClientMessage.Type == MessageType.Private)
                    {
                        await _hub.Clients.Client(clientAck.ReceiverConnectionId)
                            .SendAsync(clientAck.RetryMethod,
                            clientAck.ClientMessage.MessageId,
                            clientAck.ClientMessage.Sender,
                            clientAck.ClientMessage.Receiver,
                            clientAck.ClientMessage.Text,
                            clientAck.ClientMessage.SendTime,
                            clientAck.ClientAckId);
                    }
                    
                }
            }
        }
    }
}
