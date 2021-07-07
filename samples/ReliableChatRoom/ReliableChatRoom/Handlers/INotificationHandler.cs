using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    /// <summary>
    /// A class that handles the notification pushing (Possibly with the help of Azure Notification Hub)
    /// </summary>
    public interface INotificationHandler
    {
        /// <summary>
        /// Send private message notification.
        /// Only send to receiver.
        /// </summary>
        /// <param name="privateMessage">The message that cause the notification pushing</param>
        /// <returns></returns>
        Task SendPrivateNotification(Message privateMessage);

        /// <summary>
        /// Send broadcast message notification.
        /// Send to everybody with an active session but the sender.
        /// </summary>
        /// <param name="broadcastMessage">The message that cause the notification pushing</param>
        /// <returns></returns>
        Task SendBroadcastNotification(Message broadcastMessage);
    }
}
