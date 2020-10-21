using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class UserHandler : IUserHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, Session> _sessionTable =
            new ConcurrentDictionary<string, Session>();
        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);

        private readonly Timer _sessionCheckingTimer;
        private readonly TimeSpan _sessionExpireThreshold = TimeSpan.FromSeconds(100);
        private readonly TimeSpan _sessionCheckingInterval = TimeSpan.FromSeconds(100);
        
        public UserHandler()
        {
            this._sessionCheckingTimer = new Timer(_ => CheckSession(), state: null, dueTime: TimeSpan.FromMilliseconds(0), period: _sessionCheckingInterval);
        }

        public void Dispose()
        {
            _sessionCheckingTimer.Dispose();
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

        public ICollection<Session> GetActiveSessions()
        {
            ICollection<Session> activeSessions = new List<Session>();
            foreach (Session session in _sessionTable.Values)
            {
                if (session.SessionType == SessionTypeEnum.Active)
                {
                    activeSessions.Add(session);
                }
            }
            return activeSessions;
        }

        public Session Login(string username, string connectionId, string deviceUuid)
        {
            bool isStoredSession = _sessionTable.TryGetValue(username, out Session storedSession);
            if (isStoredSession)
            {
                storedSession.Revive(connectionId, deviceUuid);
                return storedSession;
            } else
            {
                Session session = new Session(username, connectionId, deviceUuid);
                return _sessionTable.AddOrUpdate(username, session, (k, v) => session);
            }
        }

        public DateTime Touch(string username, string connectionId, string deviceUuid)
        {
            bool isStoredSession = _sessionTable.TryGetValue(username, out Session storedSession);

            if (!isStoredSession)
            {
                return _defaultDateTime;
            }

            if (storedSession.SessionType == SessionTypeEnum.Expired) //  You cannot touch an expired session
            {
                return _defaultDateTime;
            }

            if (!connectionId.Equals(storedSession.ConnectionId)) //  ConnectionIds between two continuous touches changed
            {
                Console.WriteLine(string.Format("Touch username: {0}\nconnectionId old: {1}\nconnectionId new: {2}", username, storedSession.ConnectionId, connectionId));
                //  Update connectionId
                storedSession.ConnectionId = connectionId;
            }

            storedSession.LastTouchedDateTime = DateTime.UtcNow;
            
            return storedSession.LastTouchedDateTime;
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
