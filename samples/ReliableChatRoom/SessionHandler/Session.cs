namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class Session
    {
        public string SessionId { get; }

        public Session(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
