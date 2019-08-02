using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class MessageBox
    {
        ConcurrentDictionary<string, UserMessage> _messageBox = new ConcurrentDictionary<string, UserMessage>();

        public void addUser(string userId)
        {
            _messageBox.TryAdd(userId, new UserMessage());
        }

        public UserMessage getUserMessage(string userId)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            return result;
        }

        public void popTempMessage(string userId)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.popTempMessage();
            _messageBox.TryUpdate(userId, _result, result);
        }

        public void addHistoryMessage(string userId, Message message)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.addHistoryMessage(message);
            _messageBox.TryUpdate(userId, _result, result);
        }

        public void addTempMessage(string userId, Message message)
        {
            _messageBox.TryGetValue(userId, out UserMessage result);
            UserMessage _result = result;
            _result.addTempMessage(message);
            _messageBox.TryUpdate(userId, _result, result);
        }
    }
    public class UserMessage
    {
        ConcurrentStack<Message> historyMessage = new ConcurrentStack<Message>();
        ConcurrentQueue<Message> tempMessage = new ConcurrentQueue<Message>();

        public bool isTempEmpty()
        {
            return tempMessage.IsEmpty;
        }

        public void addTempMessage(Message message)
        {
            tempMessage.Enqueue(message);
        }

        public void popTempMessage()
        {
            if (tempMessage.IsEmpty)
            {
                return;
            }
            tempMessage.TryDequeue(out Message result);
            historyMessage.Push(result);
        }

        public Message getTempMessage()
        {
            tempMessage.TryPeek(out Message result);
            return result;
        }

        public void addHistoryMessage(Message message)
        {
            historyMessage.Push(message);
        }
    }
}