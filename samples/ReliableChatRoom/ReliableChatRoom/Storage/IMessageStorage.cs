using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public delegate Task TryStoreSucceededCallback(Message message, IHubContext<ReliableChatRoomHub> hubContext);
    public delegate Task GetHistoryMessageSucceededCallback(List<Message> historyMessages, IHubContext<ReliableChatRoomHub> hubContext);

    public interface IMessageStorage
    {
        Task<bool> TryStoreMessageAsync(Message message, TryStoreSucceededCallback callback);
        Task<bool> GetHistoryMessageAsync(string username, DateTime endDateTime, GetHistoryMessageSucceededCallback callback);
    }
}
