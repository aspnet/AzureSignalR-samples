using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.AckableChatRoom
{
    public class AckableChatRoom : Hub
    {
        private readonly IAckHandler _ackHandler;

        public AckableChatRoom(IAckHandler ackHandler)
        {
            _ackHandler = ackHandler;
        }

        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        //  Complete the task specified by the ackId.
        public void AckResponse(string ackId)
        {
            _ackHandler.Ack(ackId);
        }

        //  Send the message to the receiver
        public async Task<string> SendUserMessage(string id, string receiver, string message)
        {
            //  Create a task and wait for the receiver client to complete it.
            var ackInfo = _ackHandler.CreateAck();
            var sender = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
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
