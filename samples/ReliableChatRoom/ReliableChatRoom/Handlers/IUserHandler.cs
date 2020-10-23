using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{

    /// <summary>
    /// Defines behaviors of a user session manager service
    /// </summary>
    public interface IUserHandler
    {
        /// <summary>
        /// Registers client's username, connectionId, and deviceUuid into alive sessions
        /// </summary>
        /// <param name="username"></param>
        /// <param name="connectionId"></param>
        /// <param name="deviceUuid"></param>
        /// <returns></returns>
        Session Login(string username, string connectionId, string deviceUuid);

        /// <summary>
        /// Refreshes/ extends/ keeps alive user session
        /// </summary>
        /// <param name="username"></param>
        /// <param name="connectionId"></param>
        /// <param name="deviceUuid"></param>
        /// <returns></returns>
        DateTime Touch(string username, string connectionId, string deviceUuid);

        /// <summary>
        /// Unregisters a client
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Session Logout(string connectionId);

        /// <summary>
        /// Returns a session of the provided username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Session GetUserSession(string username);

        /// <summary>
        /// Returns a collection of active user sessions
        /// </summary>
        /// <returns></returns>
        ICollection<Session> GetActiveSessions();
    }
}
