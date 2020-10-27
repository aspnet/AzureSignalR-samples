using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    /// <summary>
    /// A class for interacting with persistent storage
    /// </summary>
    public interface IPersistentStorage
    {
        /// <summary>
        /// Try to store a list of <see cref="Message"/> in persistent storage.
        /// </summary>
        /// <param name="messages">A list of message to store</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Storage was success
        /// false - Storage was failure
        /// </returns>
        Task<bool> TryStoreMessageListAsync(List<Message> messages);

        /// <summary>
        /// Try to load persistent contents into a list of <see cref="Message"/>,
        /// according to startDateTime (included) and endDateTime (excluded).
        /// </summary>
        /// <param name="startDateTime">startDateTime (included)</param>
        /// <param name="endDateTime">endDateTime (excluded)</param>
        /// <returns>
        /// An Async Task of list of messages
        /// </returns>
        Task<List<Message>> TryLoadMessageListAsync(DateTime startDateTime, DateTime endDateTime);
    }
}
