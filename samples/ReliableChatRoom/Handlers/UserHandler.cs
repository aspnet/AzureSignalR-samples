using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class UserHandler : IUserHandler
    {
        private readonly ConcurrentDictionary<string, Session> _sessionTable =
            new ConcurrentDictionary<string, Session>();
        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);

        private readonly Timer _sessionCheckingTimer;
        private readonly TimeSpan _sessionExpireThreshold = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _sessionCheckingInterval = TimeSpan.FromSeconds(10);
        
        public UserHandler()
        {
            this._sessionCheckingTimer = new Timer(_ => CheckSession(), state: null, dueTime: TimeSpan.FromMilliseconds(0), period: _sessionCheckingInterval);
        }

        public Session GetUserSession(string username)
        {
            bool hasUser = _sessionTable.TryGetValue(username, out Session storedSession);
            if (hasUser)
            {
                return storedSession;
            }
            return null;
        }

        public Session Login(string username, string connectionId, string deviceToken)
        {
            bool isExpiredSession = _sessionTable.TryGetValue(username, out Session storedSession);
            if (isExpiredSession)
            {
                storedSession.Revive(connectionId, deviceToken);
                return storedSession;
            } else
            {
                Session session = new Session(username, connectionId, deviceToken);
                return _sessionTable.AddOrUpdate(username, session, (k, v) => session);
            }
        }

        public DateTime Touch(string username, string connectionId, string deviceToken)
        {
            if (!_sessionTable.ContainsKey(username))
            {
                return _defaultDateTime;
            }

            Session session = _sessionTable[username];

            if (session.SessionType == SessionTypeEnum.Expired)
            {
                return _defaultDateTime;
            }

            if (!connectionId.Equals(session.ConnectionId))
            {
                Console.WriteLine(string.Format("Touch username: {0}\nconnectionId old: {1}\nconnectionId new: {2}", username, session.ConnectionId, connectionId));
                session.ConnectionId = connectionId;
            }

            session.LastTouchedDateTime = DateTime.UtcNow;
            
            return session.LastTouchedDateTime;
        }

        public Session Logout(string connectionId)
        {
            string username = "";
            foreach (var pair in _sessionTable)
            {
                username = pair.Key;
                if (pair.Value.ConnectionId.Equals(connectionId))
                {
                    break;
                }
            }
            bool removalSucceeded = _sessionTable.TryRemove(username, out Session removedSession);
            if (removalSucceeded)
            {
                return removedSession;
            }
            return null;
        }

        private void CheckSession()
        {
            foreach (var pair in _sessionTable) {
                string username = pair.Key;
                Session session = pair.Value;
                if (session.SessionType == SessionTypeEnum.Active)
                {
                    var elapsed = DateTime.UtcNow - session.LastTouchedDateTime;
                    if (elapsed > _sessionExpireThreshold)
                    {
                        Console.WriteLine(string.Format("Session username: {0} time out. Force expire.", session.Username));
                        session.Expire();
                    }
                }
            }
        }
    }
}
