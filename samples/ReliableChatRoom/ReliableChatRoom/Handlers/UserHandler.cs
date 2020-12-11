using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class UserHandler : IUserHandler, IDisposable
    {
        private readonly ILogger _logger;

        /// In memory storage of user <see cref="Session"/> 
        private readonly ConcurrentDictionary<string, Session> _sessionTable =
            new ConcurrentDictionary<string, Session>();
        
        // UNIX origin of time
        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);

        /// Max time a user can keep his session alive without calling <see cref="ReliableChatRoomHub.TouchServer(string, string)"/>
        private readonly TimeSpan _sessionExpireThreshold = TimeSpan.FromSeconds(100);

        // Period of Timer checking the session
        private readonly TimeSpan _sessionCheckingInterval = TimeSpan.FromSeconds(100);

        // Timer checking the session
        private readonly Timer _sessionCheckingTimer;

        public UserHandler(ILogger<UserHandler> logger)
        {
            _logger = logger;
            _sessionCheckingTimer = new Timer(_ => CheckSession(), state: null, dueTime: TimeSpan.FromMilliseconds(0), period: _sessionCheckingInterval);
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
                // If session exists update the connectionId and deviceUuid
                storedSession.Revive(connectionId, deviceUuid);
                return storedSession;
            } else
            {
                // Otherwise, create a new Session instance
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
                _logger.LogInformation("Touch username: {0}\nconnectionId old: {1}\nconnectionId new: {2}", username, storedSession.ConnectionId, connectionId);
                //  Update connectionId
                storedSession.ConnectionId = connectionId;
            }

            storedSession.LastTouchedDateTime = DateTime.UtcNow;
            
            return storedSession.LastTouchedDateTime;
        }

        public Session Logout(string username)
        {
            bool removalSucceeded = _sessionTable.TryRemove(username, out Session removedSession);
            if (removalSucceeded)
            {
                return removedSession;
            }
            return null;
        }

        /// <summary>
        /// Called by Timer to check sessions.
        /// </summary>
        private void CheckSession()
        {
            foreach (var pair in _sessionTable) {
                Session session = pair.Value;
                if (session.SessionType == SessionTypeEnum.Active)
                {
                    var elapsed = DateTime.UtcNow - session.LastTouchedDateTime;
                    if (elapsed > _sessionExpireThreshold)
                    {
                        _logger.LogInformation("Session username: {0} time out. Force expire.", session.Username);
                        session.Expire();
                    }
                }
            }
        }
    }
}
