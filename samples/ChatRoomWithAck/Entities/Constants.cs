namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public enum MessageType
    {
        UserToUser,
        System,
        Broadcast
    }

    public enum MessageStatus
    {
        Arrived,
        Acknowledged
    }

    public class LoadMessageResult
    {
        public const string NoMessage = "You have no new messages!";

        public const string Success = "Success loading new messages!";
    }
}
