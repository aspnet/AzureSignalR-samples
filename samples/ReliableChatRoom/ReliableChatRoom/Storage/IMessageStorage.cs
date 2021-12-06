﻿
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public interface IMessageStorage
    {
        /// <summary>
        /// Try to store a <see cref="Message"/> into message storage.
        /// </summary>
        /// <param name="message">Message to store</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Storage and callback were success
        /// false - Any of above two was a failure
        /// </returns>
        Task<bool> TryStoreMessageAsync(Message message);

        /// <summary>
        /// Try to update a <see cref="Message"/> into message storage.
        /// </summary>
        /// <param name="message">Message to store</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Storage and callback were success
        /// false - Any of above two was a failure
        /// </returns>
        Task<bool> TryUpdateMessageAsync(Message message);

        /// <summary>
        /// Try to fetch a list of history messages according to the username, endDateTime provided by a client
        /// </summary>
        /// <param name="username">Client's username</param>
        /// <param name="endDateTime">DateTime of the oldest message the client currently has</param>
        /// <param name="historyMessages">Result of fetching</param>
        /// <returns>
        /// An Async Task of bool result.
        /// true - Fetch and callback were success
        /// false - Any of above two was a failure
        /// </returns>
        Task<bool> TryFetchHistoryMessageAsync(string username, DateTime endDateTime, List<Message> historyMessages);

        /// <summary>
        /// Try to fetch the content of an image with given messageId
        /// </summary>
        /// <param name="messageId">This messageId identifies uniquely the stored image blob</param>
        /// <returns></returns>
        Task<string> TryFetchImageContentAsync(string messageId);

        /// <summary>
        /// Try to fetch content of a Message with give messageId
        /// </summary>
        /// <param name="messageId">This messageId identifies uniquely the stored Message record</param>
        /// <returns></returns>
        Task<Message> TryFetchMessageById(string messageId);
    }
}
