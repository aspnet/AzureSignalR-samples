using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    /// <summary>
    /// Defines an enum class representing active session, and expired session
    /// </summary>
    public enum SessionTypeEnum
    {
        Active,
        Expired
    }
}
