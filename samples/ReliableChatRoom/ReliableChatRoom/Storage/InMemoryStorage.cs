using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public class InMemoryStorage : IMessageStorage
    {
        private readonly ConcurrentDictionary<string, Message> _messageTable =
            new ConcurrentDictionary<string, Message>();
        private readonly int _maxTableSize = 5;

        private readonly IPersistentStorage _persistentStorage;
        private DateTime _lastLoadDateTime;

        public InMemoryStorage(IPersistentStorage persistentStorage)
        {
            _persistentStorage = persistentStorage;
            _lastLoadDateTime = DateTime.UtcNow;
        }

        public List<Message> GetHistoryMessage(string username, DateTime endDateTime)
        {
            List<Message> historyMessage = new List<Message>();

            DateTime startDateTime = endDateTime - TimeSpan.FromSeconds(60);

            if (startDateTime < _lastLoadDateTime)
            {
                LoadToDictionary(startDateTime);
            }

            foreach (Message message in _messageTable.Values)
            {
                if (message.SendTime >= startDateTime && message.SendTime < endDateTime)
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
            return historyMessage;
        }

        public bool TryStoreMessage(Message message)
        {
            Console.WriteLine("TryStoreMessage");
            bool success = _messageTable.TryAdd(message.MessageId, message);
            if (success)
            {
                if (_messageTable.Count > _maxTableSize)
                {
                    Console.WriteLine(string.Format("Before persist size: {0}", _messageTable.Count));
                    Persist(message.SendTime);
                    Console.WriteLine(string.Format("After persist size: {0}", _messageTable.Count));
                }
            }
            return false;
        }

        private void Persist(DateTime endDateTime)
        {
            HashSet<string> keysToRemove = new HashSet<string>();
            List<Message> valuesToPersist = new List<Message>();
            foreach (var pair in _messageTable)
            { 
                if (pair.Value.SendTime < endDateTime)
                {
                    keysToRemove.Add(pair.Key);
                    valuesToPersist.Add(pair.Value);
                }
            }

            bool success = _persistentStorage.TryStoreMessageList(valuesToPersist);
            if (success)
            {
                foreach (string key in keysToRemove)
                {
                    _messageTable.TryRemove(key, out Message _);
                }
                _lastLoadDateTime = endDateTime;
            }
        }

        private void LoadToDictionary(DateTime startDateTime)
        {
            bool success = _persistentStorage.TryLoadMessageList(startDateTime, _lastLoadDateTime, out List<Message> outMessages);
            if (success)
            {
                Console.WriteLine(string.Format("LoadToDictionary {0} messages loaded", outMessages.Count));
                foreach (var message in outMessages)
                {
                    _messageTable.TryAdd(message.MessageId, message);
                }
                _lastLoadDateTime = startDateTime;
            }
        }
    }
}
