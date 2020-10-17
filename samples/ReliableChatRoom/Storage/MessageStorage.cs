using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public class MessageStorage : IMessageStorage
    {
        private readonly ConcurrentDictionary<string, Message> _messageTable =
            new ConcurrentDictionary<string, Message>();

        public List<Message> GetHistoryMessage(string username, DateTime until, int offset, int count)
        {
            List<Message> historyMessage = new List<Message>();
            foreach (Message message in _messageTable.Values)
            {
                if (message.SendTime > until)
                {
                    if (message.Type == MessageType.Broadcast)
                    {
                        historyMessage.Add(message);
                    }
                    else if (message.Type == MessageType.Private && message.Receiver.Equals(username))
                    {
                        historyMessage.Add(message);
                    }
                }
            }
            historyMessage.Sort((p, q) => q.SendTime.CompareTo(p.SendTime));
            return historyMessage.GetRange(offset, count);
        }

        public List<Message> GetUnreadMessage(string username, DateTime until) 
        {
            List<Message> unreadMessage = new List<Message>();
            foreach (Message message in _messageTable.Values) {
                if (message.SendTime > until)
                {
                    if (message.Type == MessageType.Broadcast)
                    {
                        unreadMessage.Add(message);
                    } else if (message.Type == MessageType.Private && message.Receiver.Equals(username))
                    {
                        unreadMessage.Add(message);
                    }
                }
            }
            unreadMessage.Sort((p, q) => p.SendTime.CompareTo(q.SendTime));
            return unreadMessage;
        }

        public bool TryStoreMessage(Message message)
        {
            return _messageTable.TryAdd(message.MessageId, message);
        }
    }
}
