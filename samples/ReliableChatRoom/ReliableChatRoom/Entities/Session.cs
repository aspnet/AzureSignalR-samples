using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public class Session
    {
        // The username provided by the client (by calling EnterChatRoom in the Hub)
        public string Username { get; set; }

        // The SignalR connection id
        public string ConnectionId { get; set; }

        // The unique device id provided by the client (by calling EnterChatRoom in the Hub), 
        // for the purpose of notification pushing.
        public string DeviceUuid { get; set; }

        // Time when the client last touched (tried to refresh his/her session in the server)
        public DateTime LastTouchedDateTime { get; set; }

        /// <see cref="SessionTypeEnum"/>>
        public SessionTypeEnum SessionType { get; set; }
        
        public Session(string username, string connectionId, string deviceUuid)
        {
            this.Username = username;
            this.ConnectionId = connectionId;
            this.DeviceUuid = deviceUuid;
            this.LastTouchedDateTime = DateTime.UtcNow;
            this.SessionType = SessionTypeEnum.Active;
        }

        /// <summary>
        /// An operation that sets the session type to <see cref="SessionTypeEnum.Expired"/>.
        /// Usually called by an IUserHandler.
        /// </summary>
        public void Expire()
        {
            this.SessionType = SessionTypeEnum.Expired;
        }

        /// <summary>
        /// An operation that sets the session type to <see cref="SessionTypeEnum.Active"/>,
        /// and then updates the connectionId and deviceUuid.
        /// Usually called by an IUserHandler.
        /// </summary>
        /// <param name="connectionId">The new connecton id</param>
        /// <param name="deviceUuid">The new device uuid</param>
        public void Revive(string connectionId, string deviceUuid)
        {
            this.SessionType = SessionTypeEnum.Active;
            this.ConnectionId = connectionId;
            this.DeviceUuid = deviceUuid;
        }

    }
}
