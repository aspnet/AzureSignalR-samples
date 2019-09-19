using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class MessageStorageInMemory : IMessageHandler
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Message>> _messageDictionary;

        public MessageStorageInMemory()
        {
            _messageDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<int, Message>>(); 
        }

        public Task<int> AddNewMessageAsync(string sessionId, Message message)
        {
            if (!_messageDictionary.ContainsKey(sessionId))
            {
                _messageDictionary.TryAdd(sessionId, new ConcurrentDictionary<int, Message>());
            }
            var sessionMessage = _messageDictionary[sessionId];

            var sequenceId = sessionMessage.Count;
            sessionMessage.TryAdd(sequenceId, message);

            _messageDictionary.AddOrUpdate(sessionId, sessionMessage, (k, v) => v);

            return Task.FromResult(sequenceId);
        }

        public Task UpdateMessageAsync(string sessionId, int sequenceId, string messageStatus)
        {
            if (!_messageDictionary.ContainsKey(sessionId))
            {
                _messageDictionary.TryAdd(sessionId, new ConcurrentDictionary<int, Message>());
            }
            var sessionMessage = _messageDictionary[sessionId];

            var message = sessionMessage[sequenceId];
            message.MessageStatus = messageStatus;
            sessionMessage.AddOrUpdate(sequenceId, message, (k,v)=>v);
            _messageDictionary.AddOrUpdate(sessionId, sessionMessage, (k, v) => v);

            return Task.CompletedTask;
        }

        public Task<List<Message>> LoadHistoryMessageAsync(string sessionId, int startSequenceId, int endSequenceId)
        {
            if (!_messageDictionary.ContainsKey(sessionId))
            {
                _messageDictionary.TryAdd(sessionId, new ConcurrentDictionary<int, Message>());
            }
            var sessionMessage = _messageDictionary[sessionId];

            // TODO: Select the messages by 2 sequenceId params

            var result = new List<Message>(sessionMessage.Values);

            return Task.FromResult(result);
        }
    }
}
