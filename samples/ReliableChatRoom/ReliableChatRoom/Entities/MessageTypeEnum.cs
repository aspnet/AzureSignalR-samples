using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    /// <summary>
    /// Defines an enum class representing private messags, system message, and broadcast message
    /// </summary>
    public enum MessageTypeEnum
    {
        Private,
        System,
        Broadcast
    }
}
