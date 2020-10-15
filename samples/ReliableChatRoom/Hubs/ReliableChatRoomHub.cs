using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs
{
    public class ReliableChatRoomHub : Hub
    {
        private readonly IAckHandler _ackHandler;
        private readonly ILoginHandler _loginHandler;

        public ReliableChatRoomHub(IAckHandler ackHandler, ILoginHandler loginHandler)
        {
            _ackHandler = ackHandler;
            _loginHandler = loginHandler;
        }

        public void EnterChatRoom(string deviceToken, string username)
        {
            (string storedConnectionId, string storedDeviceToken) = _loginHandler.Login(username, Context.ConnectionId, deviceToken);
            if (storedConnectionId.Equals(Context.ConnectionId) &&
                storedDeviceToken.Equals(deviceToken))
            {
                Clients.All.SendAsync("broadcastEnterMessage", username);
            }
        }

        public void LeaveChatRoom(string deviceToken, string username)
        {
            _loginHandler.Logout(username);
            Clients.All.SendAsync("broadcastLeaveMessage", username);
        }

        //  Complete the task specified by the ackId.
        public void AckResponse(string ackId)
        {
            _ackHandler.Ack(ackId);
        }

        //  Send the message to the receiver
        public async Task<string> SendUserMessage(string id, string sender, string receiver, string message)
        {
            //  Create a task and wait for the receiver client to complete it.
            var ackInfo = _ackHandler.CreateAck();
            await Clients.User(receiver).SendAsync("displayUserMessage", id, sender, message, ackInfo.AckId);

            //  Return the task result to the client.
            return (await ackInfo.AckTask).ToString();
        }

        // Send a customized receipt to the message sender.
        public async Task<string> SendUserAck(string msgId, string sourceName, string message)
        {
            var ackInfo = _ackHandler.CreateAck();
            await Clients.User(sourceName).SendAsync("displayAckMessage", msgId, message, ackInfo.AckId);

            return (await ackInfo.AckTask).ToString();
        }
    }
}
