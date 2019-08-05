using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class StaticMessageStorage : IMessageHandler
    {
        private ConcurrentDictionary<string, UserMessage> _messageBox = new ConcurrentDictionary<string, UserMessage>();

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
            ConcurrentStack<Message> historyMessage = new ConcurrentStack<Message>();
            ConcurrentQueue<Message> unreadMessage = new ConcurrentQueue<Message>();

            public bool IsUnreadEmpty()
            {
                return unreadMessage.IsEmpty;
            }

            public void AddUnreadMessage(Message message)
            {
                unreadMessage.Enqueue(message);
            }

            public void PopUnreadMessage()
            {
                if (unreadMessage.IsEmpty)
                {
                    return;
                }
                unreadMessage.TryDequeue(out Message result);
                historyMessage.Push(result);
            }

            public Message PeekUnreadMessage()
            {
                unreadMessage.TryPeek(out Message result);
                return result;
            }

            public void AddHistoryMessage(Message message)
            {
                historyMessage.Push(message);
            }
        }
    }
}
