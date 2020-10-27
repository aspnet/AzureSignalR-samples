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
        /// <param name="username">Client's username</param>
        /// <param name="connectionId">Client's connetion id in the current context</param>
        /// <param name="deviceUuid">Client-generated unique id for device</param>
        /// <returns>A user session <see cref="Session"/></returns>
        Session Login(string username, string connectionId, string deviceUuid);

        /// <summary>
        /// Refreshes/extends/keeps alive user session
        /// Also updates connectionId c(if changed)
        /// </summary>
        /// <param name="username">Client's username</param>
        /// <param name="connectionId">Client's connetion id in the current context</param>
        /// <param name="deviceUuid">Client-generated unique id for device</param>
        /// <returns>If touch was a success, return the DateTime of the touch. Otherwise, return default DateTime.</returns>
        DateTime Touch(string username, string connectionId, string deviceUuid);

        /// <summary>
        /// Unregisters a client
        /// </summary>
        /// <param name="connectionId">Client's connetion id in the current context</param>
        /// <returns>A user session <see cref="Session"/></returns>
        Session Logout(string connectionId);

        /// <summary>
        /// Returns a session of the provided username
        /// </summary>
        /// <param name="username">Client's username</param>
        /// <returns>A user session <see cref="Session"/></returns>
        Session GetUserSession(string username);

        /// <summary>
        /// Returns a collection of active user sessions
        /// </summary>
        /// <returns>A collection of user sessions</returns>
        ICollection<Session> GetActiveSessions();
    }
}
