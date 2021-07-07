using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    /// <summary>
    /// A class that handles the <see cref="ClientAck"/> management.
    /// </summary>
    public interface IClientAckHandler
    {
        /// <summary>
        /// Creates a <see cref="ClientAck"/> according to a <see cref="Message"/>
        /// </summary>
        /// <param name="message">The message that the Client Ack is waiting for.</param>
        /// <returns>The created ClientAck <see cref="ClientAck"/> for the message.</returns>
        ClientAck CreateClientAck(Message message);

        /// <summary>
        /// Ack and complete the <see cref="ClientAck"/> with a clientAckId.
        /// </summary>
        /// <param name="clientAckId">The unique id that specifies a <see cref="ClientAck"/></param>
        /// <param name="username">The ack sender's username</param>
        void Ack(string clientAckId, string username);
    }
}
