using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities
{
    public class Session
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public string DeviceToken { get; set; }
        public DateTime LastTouchedDateTime { get; set; }
        public SessionTypeEnum SessionType { get; set; }
        
        public Session(string username, string connectionId, string deviceToken)
        {
            this.Username = username;
            this.ConnectionId = connectionId;
            this.DeviceToken = deviceToken;
            this.LastTouchedDateTime = DateTime.UtcNow;
            this.SessionType = SessionTypeEnum.Active;
        }

        public void Expire()
        {
            this.SessionType = SessionTypeEnum.Expired;
        }

        public void Revive(string connectionId, string deviceToken)
        {
            this.SessionType = SessionTypeEnum.Active;
            this.ConnectionId = connectionId;
            this.DeviceToken = deviceToken;
        }

    }
}
