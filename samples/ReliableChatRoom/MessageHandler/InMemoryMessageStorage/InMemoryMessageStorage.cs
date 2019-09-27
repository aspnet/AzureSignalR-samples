using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class InMemoryMessageStorage : IMessageHandler
    {
        private readonly ConcurrentDictionary<string, SessionMessage> _messageDictionary;

        public InMemoryMessageStorage()
        {
            _messageDictionary = new ConcurrentDictionary<string, SessionMessage>(); 
        }

        public Task<string> AddNewMessageAsync(string sessionId, Message message)
        {
            lock (_messageDictionary)
            {
                if (!_messageDictionary.ContainsKey(sessionId))
                {
                    _messageDictionary.TryAdd(sessionId, new SessionMessage());
                }
                var sessionMessage = _messageDictionary[sessionId];

                var sequenceId = sessionMessage.TryAddMessage(message);

                return Task.FromResult(sequenceId.ToString());
            }
        }

        public Task UpdateMessageAsync(string sessionId, string sequenceId, string messageStatus)
        {
            lock (_messageDictionary)
            {
                if (!_messageDictionary.TryGetValue(sessionId, out var sessionMessage))
                {
                    throw new Exception("Session not found!");
                }

                sessionMessage.TryUpdateMessage(int.Parse(sequenceId), messageStatus);

                return Task.CompletedTask;
            }
        }

        public Task<List<Message>> LoadHistoryMessageAsync(string sessionId)
        {
            lock (_messageDictionary)
            {
                if (!_messageDictionary.TryGetValue(sessionId, out var sessionMessage))
                {
                    _messageDictionary.TryAdd(sessionId, new SessionMessage());
                    return Task.FromResult(new List<Message>());
                }

                var result = new List<Message>(sessionMessage.Messages.ToList());
                result.Sort();

                return Task.FromResult(result);
            }
        }

        internal class SessionMessage
        {
            public const int MAX_SIZE = 1000;

            public int LastSequenceId { get; set; }

            public List<Message> Messages { get; set; }

            public SessionMessage()
            {
                LastSequenceId = -1;
                Messages = new List<Message>(MAX_SIZE);
            }

            public int TryAddMessage(Message message)
            {
                LastSequenceId++;
                message.SequenceId = LastSequenceId.ToString();

                if (LastSequenceId < MAX_SIZE)
                {
                    Messages.Add(message);
                }
                else
                {
                    Messages[LastSequenceId % MAX_SIZE] = message;
                }

                return LastSequenceId;
            }

            public void TryUpdateMessage(int sequenceId, string messageStatus)
            {
                if (sequenceId <= LastSequenceId - MAX_SIZE || sequenceId > LastSequenceId || sequenceId >= 0) 
                {
                    throw new Exception("Message not found");
                }

                Messages[sequenceId % MAX_SIZE].MessageStatus = messageStatus;

                return;
            }
        }
    }
}
