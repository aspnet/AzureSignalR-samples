namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public enum MessageType
    {
        Private,
        System,
        Broadcast
    }

    public enum MessageStatus
    {
        Arrived,
        Acknowledged,
        Read
    }

    public class LoadMessageResult
    {
        public const string NoMessage = "You have no new messages!";

        public const string Success = "Success loading new messages!";
    }
}
