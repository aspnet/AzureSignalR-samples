using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public enum MessageType
    {
        UserTouser,
        System,
        Broadcast
    }

    public static class AckResult
    {
        public static string Success = "Success";

        public static string Fail = "Fail";

        public static string TimeOut = "Timeout"; 

        public static string NoAck = "NoAck";
    }

    public static class MessageStatus
    {
        public static string Arrived = "Arrived"; 

        public static string Acknowledged = "Acknoeledged";
    }

    public static class LoadMessageResult
    {
        public static string NoMessage = "You have no new messages!";

        public static string Success = "Success loading new messages!";

        public static string Fail = "Please try again to load new messages!";
    }

    public static class HubString
    {
        public static string UserNotFound = "User not found!";

        public static string EchoNotification = " (echo form server)";
    }
}
