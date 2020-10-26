using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    /// <summary>
    /// Wrapper class of user messages
    /// </summary>
    public class Message
    {
        // String placeholder for the Receiver field of a Broadcast Message
        public static readonly string BROADCAST_RECEIVER = "BCAST";

        // String placeholder for the Sender field of a System Message
        public static readonly string SYSTEM_SENDER = "SYS";

        // A Uuid generated and sent by client. Server-side do not generate messageIds
        public string MessageId { get; set; }

        /// <see cref="MessageTypeEnum"/> 
        public MessageTypeEnum Type { get; set; }

        // Sender and Receiver of a message
        public string Sender { get; set; }
        public string Receiver { get; set; }

        // Content of message. Can be either a text string or rich content represented by a Base64 string
        public string Payload { get; set; }

        // The time when the broadcast message reaches the server are labeled as sendTime
        public DateTime SendTime { get; set; }

        // Constructor
        public Message(string messageId, string sender, string receiver, string payload, MessageTypeEnum type, DateTime sendTime)
        {
            this.MessageId = messageId;
            this.Type = type;
            this.Sender = sender;
            this.Receiver = receiver;
            this.Payload = payload;
            this.SendTime = sendTime;
        }
    }
}
