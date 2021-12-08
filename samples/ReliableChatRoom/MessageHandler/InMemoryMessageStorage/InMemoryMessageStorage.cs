// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            if (!_messageDictionary.TryGetValue(sessionId, out var sessionMessage))
            {
                _messageDictionary.TryAdd(sessionId, new SessionMessage());
                sessionMessage = _messageDictionary[sessionId];
            }

            var sequenceId = sessionMessage.TryAddMessage(message);

            return Task.FromResult(sequenceId.ToString());
        }

        public async Task UpdateMessageAsync(string sessionId, string sequenceId, string messageStatus)
        {
            if (!_messageDictionary.TryGetValue(sessionId, out var sessionMessage))
            {
                throw new Exception("Session not found!");
            }

            await sessionMessage.TryUpdateMessage(int.Parse(sequenceId), messageStatus);

            return;
        }

        public Task<List<Message>> LoadHistoryMessageAsync(string sessionId)
        {
            if (!_messageDictionary.TryGetValue(sessionId, out var sessionMessage))
            {
                _messageDictionary.TryAdd(sessionId, new SessionMessage());
                return Task.FromResult(new List<Message>());
            }

            var result = new List<Message>(sessionMessage.Messages.Values.ToList());
            result.Sort();

            return Task.FromResult(result);
        }

        internal class SessionMessage
        {
            public int LastSequenceId;

            public ConcurrentDictionary<int, Message> Messages { get; set; }

            public SessionMessage()
            {
                LastSequenceId = -1;
                Messages = new ConcurrentDictionary<int, Message>();
            }

            public int TryAddMessage(Message message)
            {
                var sequenceId = Interlocked.Increment(ref LastSequenceId);
                message.SequenceId = sequenceId.ToString();
                Messages.TryAdd(sequenceId, message);

                return sequenceId;
            }

            public async Task TryUpdateMessage(int sequenceId, string messageStatus)
            {
                var retry = 0;
                const int MAX_RETRY = 10;

                while (retry < MAX_RETRY)
                {
                    Messages.TryGetValue(sequenceId, out var message);
                    var newMessage = message;
                    newMessage.MessageStatus = messageStatus;

                    if (Messages.TryUpdate(sequenceId, newMessage, message))
                    {
                        return;
                    }

                    ++retry;
                    await Task.Delay(new Random().Next(10, 100));
                }

                throw new Exception("Fail to update messages");
            }
        }
    }
}
