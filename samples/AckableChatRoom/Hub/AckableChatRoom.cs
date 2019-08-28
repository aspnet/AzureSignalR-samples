using System;
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
        public void Ack(string ackId)
        {
            _ackHandler.Ack(ackId);
        }

        //  Send the messageContent to the receiver
        public async Task<string> SendUserMessage(string messageId, string receiver, string messageContent)
        {
            //  Create a task and wait for the receiver client to complete it.
            var ackInfo = _ackHandler.CreateAck();
            var sender = Context.UserIdentifier;
            await Clients.User(receiver)
                .SendAsync("displayUserMessage", messageId, sender, messageContent, ackInfo.AckId);

            //  Return the task result to the client.
            return (await ackInfo.AckTask);
        }
    }
}
