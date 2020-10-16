using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public interface IClientAckHandler
    {
        ClientAck CreateClientAck(Message message);
        void Ack(string clientAckId);
        ICollection<ClientAck> GetTimeOutClientAcks();
    }
}
