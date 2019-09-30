// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public interface ISessionHandler
    {
        /// <summary>
        /// Creates a new session or loads the existed session.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="partnerName"></param>
        /// <returns>The session instance</returns>
        Task<Session> GetOrCreateSessionAsync(string userName, string partnerName);

        /// <summary>
        /// Gets all related sessions of one user.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>A list of sessions</returns>
        Task<KeyValuePair<string, Session>[]> GetLatestSessionsAsync(string userName);
    }
}
