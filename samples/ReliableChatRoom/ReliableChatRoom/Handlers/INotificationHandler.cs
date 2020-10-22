using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public interface INotificationHandler
    {
        void SendPrivateNotification(Message privateMessage);
        void SendBroadcastNotification(Message broadcastMessage);
    }
}
