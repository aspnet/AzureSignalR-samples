using Microsoft.Azure.Functions.Worker;

namespace DotnetIsolated_ClassBased;
public class NewMessage
{
    public string ConnectionId { get; }
    public string Sender { get; }
    public string Text { get; }

    public NewMessage(SignalRInvocationContext invocationContext, string message)
    {
        Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
        ConnectionId = invocationContext.ConnectionId;
        Text = message;
    }
}

