using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory
{
    public class MessageFactory : IMessageFactory
    {
        public Message CreateSystemMessage(string username, string action, DateTime sendTime)
        {
            return new Message(
                Guid.NewGuid().ToString(),
                Message.SYSTEM_SENDER, Message.BROADCAST_RECEIVER,
                string.Format("{0} has {1} the chat", username, action),
                MessageTypeEnum.System,
                sendTime);
        }

        public Message CreateBroadcastMessage(string messageId, string sender, string text, DateTime sendTime)
        {
            return new Message(
                messageId,
                sender, Message.BROADCAST_RECEIVER,
                text,
                MessageTypeEnum.Broadcast,
                sendTime);
        }

        public Message CreatePrivateMessage(string messageId, string sender, string receiver, string text, DateTime sendTime)
        {
            return new Message(
                messageId,
                sender, receiver,
                text,
                MessageTypeEnum.Private,
                sendTime);
        }

    }
}
