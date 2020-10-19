using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public class Message
    {
        public static readonly string BROADCAST_RECEIVER = "BCAST";
        public static readonly string SYSTEM_SENDER = "SYS";
        
        public string MessageId { get; set; }
        public string Text { get; set; }
        public MessageTypeEnum Type { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public DateTime SendTime { get; set; }

        public Message(string messageId, string sender, string receiver, string text, MessageTypeEnum type, DateTime sendTime)
        {
            this.MessageId = messageId;
            this.Type = type;
            this.Sender = sender;
            this.Receiver = receiver;
            this.Text = text;
            this.SendTime = sendTime;
        }

    }
}
