using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public interface IClientAckHandler
    {
        ClientAck CreateClientAck(Message message, string retryMethod, string senderConnectionId, string receiverConnectionId);
        void SetHub(Hub hub);
        void Ack(string clientAckId);
    }
}
