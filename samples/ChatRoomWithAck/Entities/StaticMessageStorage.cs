using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class StaticMessageStorage : IMessageHandler
    {
        private readonly ConcurrentDictionary<string, UserMessage> _messageBox = new ConcurrentDictionary<string, UserMessage>();

        public void AddUser(string userId)
        {
            _messageBox.TryAdd(userId, new UserMessage());
        }

        public void AddHistoryMessage(string userId, Message message)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.AddHistoryMessage(message);
            _messageBox.TryUpdate(userId, _result, result);
        }

        public void AddUnreadMessage(string userId, Message message)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.AddUnreadMessage(message);
            _messageBox.TryUpdate(userId, _result, result);
        }

        public bool IsUnreadEmpty(string userId)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            return result.IsUnreadEmpty();
        }

        public Message PeekUnreadMessage(string userId)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            return result.PeekUnreadMessage();
        }

        public void PopUnreadMessage(string userId)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.PopUnreadMessage();
            _messageBox.TryUpdate(userId, _result, result);
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
