using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public class TablePersistentStorage : IPersistentStorage
    {
        public bool TryLoadMessageList(DateTime startDateTime, DateTime endDateTime, out List<Message> outMessages)
        {
            throw new NotImplementedException();
        }

        public bool TryStoreMessageList(List<Message> messages)
        {
            throw new NotImplementedException();
        }
    }
}
