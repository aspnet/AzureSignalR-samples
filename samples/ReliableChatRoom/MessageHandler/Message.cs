using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class Message
    {
        public string SenderName { get; }

        public DateTime SendTime { get; }

        public string MessageContent { get; set; }

        public string MessageStatus { get; set; }

        public Message(string senderName, DateTime sendTime, string messageContent, string messageStatus)
        {
            SenderName = senderName;
            SendTime = sendTime;
            MessageContent = messageContent;
            MessageStatus = messageStatus;
        }
    }
}
