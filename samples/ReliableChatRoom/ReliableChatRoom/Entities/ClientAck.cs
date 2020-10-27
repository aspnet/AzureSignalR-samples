using System;
using System.Collections.Generic;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    /// <summary>
    /// A class that stores information about client acks.
    /// Also stores information that can be utilized by a <see cref="IClientAckHandler"/>
    /// to decide whether resending messages and checking acks are necessary. 
    /// </summary>
    public class ClientAck
    {
        // A unique ClientAck ID
        public string ClientAckId { get; set; }

        // Time that this specific instance of ClientAck has been retried
        public int RetryCount { get; set; }

        /// <see cref="ClientAckResultEnum"/>
        public ClientAckResultEnum ClientAckResult { get; set; }

        // Start time of a ClientAck.
        // Resending policies are applied on the calculation results based on this field.
        public DateTime ClientAckStartDateTime { get; set; }

        // For which client message this ClientAck is waiting.
        public Message ClientMessage { get; set; }

        // Username of receivers
        public List<string> Receivers { get; set; }

        public ClientAck(string clientAckId, DateTime startDateTime, Message message, List<string> receivers)
        {
            this.ClientAckId = clientAckId;
            this.RetryCount = 0;
            this.ClientAckResult = ClientAckResultEnum.Waiting;
            this.ClientAckStartDateTime = startDateTime;
            this.ClientMessage = message;
            this.Receivers = receivers;
        }

        /// <summary>
        /// An operation that retries the ClientAck
        /// </summary>
        public void Retry()
        {
            this.RetryCount += 1;
            this.ClientAckResult = ClientAckResultEnum.Waiting;
            this.ClientAckStartDateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// An operation that fails the ClientAck
        /// </summary>
        public void Fail()
        {
            this.ClientAckResult = ClientAckResultEnum.Failure;
        }

        /// <summary>
        /// An operation that times out the ClientAck
        /// </summary>
        public void TimeOut()
        {
            this.ClientAckResult = ClientAckResultEnum.TimeOut;
        }
    }
}
