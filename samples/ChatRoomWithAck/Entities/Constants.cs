using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public enum MessageType
    {
        UserToUser,
        System,
        Broadcast
    }

    public enum AckResult
    {
        Success,
        Fail,
        TimeOut,
        NoAck
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

    public class HubString
    {
        public const string EchoNotification = " (echo form server)";
    }
}
