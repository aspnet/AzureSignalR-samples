using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class ClientAckHandler : IClientAckHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, ClientAck> _clientAcks
            = new ConcurrentDictionary<string, ClientAck>();

        private readonly TimeSpan _ackThreshold;


        private Timer _timer;

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

            _ackThreshold = ackThreshold;
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
                if (clientAck.ClientAckResult == ClientAckResultEnum.Waiting)
                {
                    var elapsed = DateTime.UtcNow - clientAck.ClientAckStartDateTime;
                    if (elapsed > _ackThreshold)
                    {
                        clientAck.ClientAckResult = ClientAckResultEnum.TimeOut;
                    }
                }
            }
        }

        public ICollection<ClientAck> GetTimeOutClientAcks()
        {
            ICollection<ClientAck> timeOutClientAcks =
                new List<ClientAck>();
            foreach (var pair in _clientAcks)
            {
                string clientAckId = pair.Key;
                ClientAck clientAck = pair.Value;
                if (clientAck.ClientAckResult == ClientAckResultEnum.TimeOut)
                {
                    timeOutClientAcks.Add(clientAck);
                }
            }
            return timeOutClientAcks;
        }
    }
}
