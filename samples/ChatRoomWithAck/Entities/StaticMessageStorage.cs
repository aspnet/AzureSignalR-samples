using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class StaticMessageStorage : IMessageHandler
    {
        private static readonly ConcurrentDictionary<string, UserMessage> MessageBox = new ConcurrentDictionary<string, UserMessage>();

        private readonly ILogger<StaticMessageStorage> _logger;

        public StaticMessageStorage(ILogger<StaticMessageStorage> logger)
        {
            _logger = logger;
        }

        public void AddUser(string userId)
        {
            MessageBox.TryAdd(userId, new UserMessage());
        }

        public void AddHistoryMessage(string userId, Message message)
        {
            MessageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.AddHistoryMessage(message);
            MessageBox.TryUpdate(userId, _result, result);
        }

        public void AddUnreadMessage(string userId, Message message)
        {
            MessageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.AddUnreadMessage(message);
            MessageBox.TryUpdate(userId, _result, result);
        }

        public bool IsUnreadEmpty(string userId)
        {
            MessageBox.TryGetValue(userId, out UserMessage result);
            return result.IsUnreadEmpty();
        }

        public Message PeekUnreadMessage(string userId)
        {
            MessageBox.TryGetValue(userId, out UserMessage result);
            return result.PeekUnreadMessage();
        }

        public void PopUnreadMessage(string userId)
        {
            MessageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.PopUnreadMessage();
            MessageBox.TryUpdate(userId, _result, result);
        }

        private class UserMessage
        {
            private readonly ConcurrentStack<Message> _historyMessage = new ConcurrentStack<Message>();
            private readonly ConcurrentQueue<Message> _unreadMessage = new ConcurrentQueue<Message>();

            public bool IsUnreadEmpty()
            {
                return _unreadMessage.IsEmpty;
            }

            public void AddUnreadMessage(Message message)
            {
                _unreadMessage.Enqueue(message);
            }

            public void PopUnreadMessage()
            {
                if (_unreadMessage.IsEmpty)
                {
                    return;
                }
                _unreadMessage.TryDequeue(out Message result);
                _historyMessage.Push(result);
            }

            public Message PeekUnreadMessage()
            {
                _unreadMessage.TryPeek(out Message result);
                return result;
            }

            public void AddHistoryMessage(Message message)
            {
                _historyMessage.Push(message);
            }
        }
    }
}
