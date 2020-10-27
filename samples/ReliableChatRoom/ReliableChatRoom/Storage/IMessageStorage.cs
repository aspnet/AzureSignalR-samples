using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    /// <summary>
    /// A callback delegate called when <see cref="IMessageStorage.TryStoreMessageAsync"/> executed succeeded.
    /// </summary>
    /// <param name="message">The mesage which is successfully stored into storage</param>
    /// <param name="hubContext">SignalR HubContext for calling hub methods</param>
    /// <returns>
    /// An Async Task of bool result.
    /// true - Callback was success
    /// false - Callback was failure
    /// </returns>
    public delegate Task<bool> OnStoreSuccess(Message message, IHubContext<ReliableChatRoomHub> hubContext);

    /// <summary>
    /// A callback delegate called when <see cref="IMessageStorage.FetchHistoryMessageAsync"/> executed succeeded.
    /// </summary>
    /// <param name="historyMessages">The fetched list of history messages</param>
    /// <param name="hubContext">SignalR HubContext for calling hub methods</param>
    /// <returns>
    /// An Async Task of bool result.
    /// true - Callback was success
    /// false - Callback was failure
    /// </returns>
    public delegate Task<bool> OnFetchSuccess(List<Message> historyMessages, IHubContext<ReliableChatRoomHub> hubContext);

    public interface IMessageStorage
    {
        /// <summary>
        /// Try to store a <see cref="Message"/> into message storage.
        /// </summary>
        /// <param name="message">Message to store</param>
        /// <param name="callback">A callback method called when storage was a success</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Storage and callback were success
        /// false - Any of above two was a failure
        /// </returns>
        Task<bool> TryStoreMessageAsync(Message message, OnStoreSuccess callback);

        /// <summary>
        /// Try to fetch a list of history messages according to the username, endDateTime provided by a client
        /// </summary>
        /// <param name="username">Client's username</param>
        /// <param name="endDateTime">DateTime of the oldest message the client currently has</param>
        /// <param name="callback">A callback method called when fetch was a success</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Fetch and callback were success
        /// false - Any of above two was a failure
        /// </returns>
        Task<bool> FetchHistoryMessageAsync(string username, DateTime endDateTime, OnFetchSuccess callback);
    }
}
