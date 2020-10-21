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

        public List<Message> GetHistoryMessage(string username, string untilMessageId, int offset, int count)
        {
            List<Message> historyMessage = new List<Message>();
            DateTime until;
            if (!untilMessageId.Equals("")) {
                until = _messageTable[untilMessageId].SendTime;
            } else
            {
                until = DateTime.UtcNow;
            }

            foreach (Message message in _messageTable.Values)
            {
                if (message.SendTime < until)
                {
                    if (message.Type == MessageTypeEnum.Broadcast)
                    {
                        historyMessage.Add(message);
                    }
                    else if (message.Type == MessageTypeEnum.Private && 
                        (message.Receiver.Equals(username) || message.Sender.Equals(username)))
                    {
                        historyMessage.Add(message);
                    }
                }
            }
            historyMessage.Sort((p, q) => q.SendTime.CompareTo(p.SendTime));
            if (offset >= historyMessage.Count || offset + count > historyMessage.Count)
            {
                return historyMessage;
            }
            return historyMessage.GetRange(offset, count);
        }

        public List<Message> GetUnreadMessage(string username, string untilMessageId) 
        {
            List<Message> unreadMessage = new List<Message>();
            DateTime until;
            if (!untilMessageId.Equals(""))
            {
                until = _messageTable[untilMessageId].SendTime;
            }
            else
            {
                until = DateTime.UtcNow;
            }

            foreach (Message message in _messageTable.Values) {
                if (message.SendTime < until)
                {
                    if (message.Type == MessageTypeEnum.Broadcast)
                    {
                        unreadMessage.Add(message);
                    } else if (message.Type == MessageTypeEnum.Private && message.Receiver.Equals(username))
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
