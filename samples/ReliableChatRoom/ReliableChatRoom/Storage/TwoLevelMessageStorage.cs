using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    [Obsolete]
    public class TwoLevelMessageStorage : IMessageStorage
    {
        private readonly ConcurrentDictionary<string, Message> _messageTable =
            new ConcurrentDictionary<string, Message>();
        private readonly int _maxTableSize = 5;
        private readonly IHubContext<ReliableChatRoomHub> _hubContext;
        private readonly IPersistentStorage _persistentStorage;
        private DateTime _lastLoadDateTime;
        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);
        private TimeSpan[] _spansToTraceBack =
            {   TimeSpan.FromMinutes(1), TimeSpan.FromHours(1),
                TimeSpan.FromHours(4), TimeSpan.FromHours(12),
                TimeSpan.FromDays(1)  };

        public TwoLevelMessageStorage(IHubContext<ReliableChatRoomHub> hubContext, IPersistentStorage persistentStorage)
        {
            _hubContext = hubContext;
            _persistentStorage = persistentStorage;
            _lastLoadDateTime = DateTime.UtcNow;
        }

        public async Task<bool> TryFetchHistoryMessageAsync(string username, DateTime endDateTime, List<Message> historyMessages)
        {
            DateTime startDateTime = _defaultDateTime;
            foreach (TimeSpan traceBackSpan in _spansToTraceBack)
            {
                startDateTime = endDateTime - traceBackSpan;
                if (startDateTime < _lastLoadDateTime)
                {
                    int loadCount = await LoadToDictionary(startDateTime);
                    if (loadCount > 0)
                    {
                        break;
                    }
                }
            }

            foreach (Message message in _messageTable.Values)
            {
                if (message.SendTime >= startDateTime && message.SendTime < endDateTime)
                {
                    if (message.Type == MessageTypeEnum.Broadcast)
                    {
                        historyMessages.Add(message);
                    }
                    else if (message.Type == MessageTypeEnum.Private &&
                        (message.Receiver.Equals(username) || message.Sender.Equals(username)))
                    {
                        historyMessages.Add(message);
                    }
                }
            }


            historyMessages.Sort((p, q) => q.SendTime.CompareTo(p.SendTime));

            return true;
        }

        public async Task<bool> TryStoreMessageAsync(Message message)
        {
            Console.WriteLine("TryStoreMessage");
            bool success = _messageTable.TryAdd(message.MessageId, message);
            if (success)
            {
                if (_messageTable.Count > _maxTableSize)
                {
                    Console.WriteLine(string.Format("Before persist size: {0}", _messageTable.Count));
                    await Persist(message.SendTime);
                    Console.WriteLine(string.Format("After persist size: {0}", _messageTable.Count));
                }
            }

            return true;
        }

        private async Task Persist(DateTime endDateTime)
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

            bool success = await _persistentStorage.TryStoreMessageListAsync(valuesToPersist);
            if (success)
            {
                foreach (string key in keysToRemove)
                {
                    _messageTable.TryRemove(key, out Message _);
                }
                _lastLoadDateTime = endDateTime;
            }

        }

        private async Task<int> LoadToDictionary(DateTime startDateTime)
        {
            List<Message> historyMessages = await _persistentStorage.TryLoadMessageListAsync(startDateTime, _lastLoadDateTime);

            Console.WriteLine(string.Format("LoadToDictionary {0} messages loaded", historyMessages.Count));

            foreach (var message in historyMessages)
            {
                _messageTable.TryAdd(message.MessageId, message);
            }

            _lastLoadDateTime = startDateTime;
            return historyMessages.Count;
        }

        public async Task<string> TryFetchImageContent(string messageId)
        {
            await Task.Delay(1000);
            return "";
        }

        public Task<string> TryFetchImageContentAsync(string messageId)
        {
            throw new NotImplementedException();
        }

        public Task<Message> TryFetchMessageById(string messageId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryUpdateMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
