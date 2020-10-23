using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public interface IPersistentStorage
    {
        Task<bool> TryStoreMessageListAsync(List<Message> messages);
        Task<List<Message>> TryLoadMessageListAsync(DateTime startDateTime, DateTime endDateTime);
    }
}
