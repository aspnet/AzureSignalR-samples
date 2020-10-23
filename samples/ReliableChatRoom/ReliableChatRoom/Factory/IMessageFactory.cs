using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory
{
    public interface IMessageFactory
    {
        Message CreateSystemMessage(string username, string action, DateTime sendDate);
        Message CreateBroadcastMessage(string messageId, string sender, string text, DateTime sendTime);
        Message CreatePrivateMessage(string messageId, string sender, string receiver, string text, DateTime sendTime);
        List<Message> FromJsonString(string jsonString);
        Message FromSingleJsonString(string jsonString);
        string ToJsonString(List<Message> messages);
        string ToJsonString(Message message);
    }
}
