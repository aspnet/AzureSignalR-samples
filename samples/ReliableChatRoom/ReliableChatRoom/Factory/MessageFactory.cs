using Azure.Storage.Blobs;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Newtonsoft.Json;
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

        public List<Message> FromJsonString(string jsonString)
        {
            List<Message> messages = (List<Message>) JsonConvert.DeserializeObject(jsonString, typeof(List<Message>));
            return messages;
        }

        public Message FromSingleJsonString(string jsonString)
        {
            Message message = (Message) JsonConvert.DeserializeObject(jsonString, typeof(Message));
            return message;
        }

        public string ToJsonString(List<Message> messages)
        {
            return JsonConvert.SerializeObject(messages);
        }

        public string ToSingleJsonString(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }
}
