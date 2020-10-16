using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public class ClientAck
    {
        public string ClientAckId { get; set; }

        public int RetryCount { get; set; }

        public ClientAckResultEnum ClientAckResult { get; set; }

        public DateTime ClientAckStartDateTime { get; set; }

        public Message ClientMessage { get; set; }

        public string RetryMethod { get; set; }

        public string SenderConnectionId { set; get; }

        public string ReceiverConnectionId { set; get; }

        public ClientAck(string clientAckId, DateTime startDateTime, Message message, string retryMethod, string senderConnectionId, string receiverConnectionId)
        {
            this.ClientAckId = clientAckId;
            this.RetryCount = 0;
            this.ClientAckResult = ClientAckResultEnum.Waiting;
            this.ClientAckStartDateTime = startDateTime;
            this.ClientMessage = message;
            this.RetryMethod = retryMethod;
            this.SenderConnectionId = senderConnectionId;
            this.ReceiverConnectionId = receiverConnectionId;
        }

        public int IncRetryCount()
        {
            this.RetryCount += 1;
            return this.RetryCount;
        }
    }
}
