using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class ReliableChatRoom : Hub
    {
        private readonly IAckHandler _ackHandler;

        public ReliableChatRoom(IAckHandler ackHandler)
        {
            _ackHandler = ackHandler;
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
