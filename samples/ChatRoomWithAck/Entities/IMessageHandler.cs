namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public interface IMessageHandler
    {
        void AddUser(string userId);

        void AddHistoryMessage(string userId, Message message);

        void AddUnreadMessage(string userId, Message message);

        void PopUnreadMessage(string userId);

        Message PeekUnreadMessage(string userId);

        bool IsUnreadEmpty(string userId);
    }
}
