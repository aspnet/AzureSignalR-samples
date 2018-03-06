using Microsoft.AspNetCore.SignalR;

namespace ChatRoomAspNet
{
    public class Chat : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.InvokeAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).InvokeAsync("echo", name, message + " (echo from server)");
        }
    }
}