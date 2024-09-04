using OpenAI.Chat;
using System.Collections.Concurrent;

namespace AIStreaming
{
    public class GroupHistoryStore
    {
        private readonly ConcurrentDictionary<string, IList<ChatMessage>> _store = new();

        public IReadOnlyList<ChatMessage> GetOrAddGroupHistory(string groupName, string userName, string message)
        {
            var chatMessages = _store.GetOrAdd(groupName, _ => InitiateChatMessages());
            chatMessages.Add(new UserChatMessage(GenerateUserChatMessage(userName, message)));
            return chatMessages.AsReadOnly();
        }

        public void UpdateGroupHistoryForAssistant(string groupName, string message)
        {
            var chatMessages = _store.GetOrAdd(groupName, _ => InitiateChatMessages());
            chatMessages.Add(new AssistantChatMessage(message));
        }

        private IList<ChatMessage> InitiateChatMessages()
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a friendly and knowledgeable assistant participating in a group discussion." +
                " Your role is to provide helpful, accurate, and concise information when addressed." +
                " Maintain a respectful tone, ensure your responses are clear and relevant to the group's ongoing conversation, and assist in facilitating productive discussions." +
                " Messages from users will be in the format 'UserName: chat messages'." +
                " Pay attention to the 'UserName' to understand who is speaking and tailor your responses accordingly."),
            };
            return messages;
        }

        private string GenerateUserChatMessage(string userName, string message)
        {
            return $"{userName}: {message}";
        }
    }
}
