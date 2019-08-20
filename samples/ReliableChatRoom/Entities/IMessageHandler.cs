namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public interface IMessageHandler
    {
        void AddHistoryMessage(string userId, Message message);

        void AddUnreadMessage(string userId, Message message);

        void PopUnreadMessage(string userId);

        Message PeekUnreadMessage(string userId);

        bool IsUnreadEmpty(string userId);
    }
}
