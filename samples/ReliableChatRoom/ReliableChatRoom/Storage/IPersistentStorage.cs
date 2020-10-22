using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public interface IPersistentStorage
    {
        bool TryStoreMessageList(List<Message> messages);
        bool TryLoadMessageList(DateTime startDateTime, DateTime endDateTime, out List<Message> outMessages);
    }
}
