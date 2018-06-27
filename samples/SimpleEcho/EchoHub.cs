using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR.Samples.SimpleEcho
{
    public class EchoHub : Hub
    {
        public void Echo(string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", message);
        }
    }
}
