using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public class ClientAck
    {
        public string ClientAckId { get; set; }

        public int RetryCount { get; set; }

        public ClientAckResultEnum ClientAckResult { get; set; }

        public DateTime ClientAckStartDateTime { get; set; }

        public Message ClientMessage { get; set; }

        public ClientAck(string clientAckId, DateTime startDateTime, Message message)
        {
            this.ClientAckId = clientAckId;
            this.RetryCount = 0;
            this.ClientAckResult = ClientAckResultEnum.Waiting;
            this.ClientAckStartDateTime = startDateTime;
            this.ClientMessage = message;
        }

        public void Retry()
        {
            this.RetryCount += 1;
            this.ClientAckResult = ClientAckResultEnum.Waiting;
            this.ClientAckStartDateTime = DateTime.UtcNow;
        }

        public void Fail()
        {
            this.ClientAckResult = ClientAckResultEnum.Failure;
        }

        public void TimeOut()
        {
            this.ClientAckResult = ClientAckResultEnum.TimeOut;
        }
    }
}
