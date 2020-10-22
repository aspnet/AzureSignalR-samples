using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    [Obsolete("Not used any more")]
    public interface IMessageHandler
    {
        void AddHistoryMessage(string userId, Message message);

        void AddUnreadMessage(string userId, Message message);

        void PopUnreadMessage(string userId);

        Message PeekUnreadMessage(string userId);

        bool IsUnreadEmpty(string userId);
    }
}
