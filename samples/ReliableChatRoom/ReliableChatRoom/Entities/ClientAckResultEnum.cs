namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    /// <summary>
    /// Defines an enum class representing possible states of a <see cref="ClientAck"/>
    /// </summary>
    public enum ClientAckResultEnum
    {
        Waiting,
        Success,
        TimeOut,
        Failure
    }

}
