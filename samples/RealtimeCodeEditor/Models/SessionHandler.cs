using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RealtimeCodeEditor.Hubs;
using RealtimeCodeEditor.Models.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RealtimeCodeEditor.Models
{
    public class SessionHandler : IDisposable
    {
        private static readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random random = new Random();

        private ILogger<SessionHandler> _logger;

        private ConcurrentDictionary<string, Session> _sessions;

        private readonly int _sessionCodeLength = 6;

        private readonly TimeSpan _sessionExpireThreshold = TimeSpan.FromSeconds(180);
        private readonly TimeSpan _sessionCheckingInterval = TimeSpan.FromSeconds(90);
        private Timer _sessionChecker;

        public SessionHandler(ILogger<SessionHandler> logger)
        {
            _logger = logger;
            _sessions = new ConcurrentDictionary<string, Session>();
            _sessionChecker = new Timer(_ => CheckSession(), state: null, dueTime: TimeSpan.FromMilliseconds(0), period: _sessionCheckingInterval);
        }

        private string GenerateRandomSessionCode()
        {
            return new string(Enumerable.Repeat(chars, _sessionCodeLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string CreateSession(string creator)
        {
            string sessionCode = GenerateRandomSessionCode();

            Session session = new Session(sessionCode, creator);
            _sessions.TryAdd(sessionCode, session);

            _logger.LogInformation("Create Session creator: {0}, code: {1}", creator, sessionCode);
            return sessionCode;
        }

        public bool JoinSession(string sessionCode, string user)
        {
            bool sessionExists = _sessions.TryGetValue(sessionCode, out Session session);

            if (sessionExists)
            {
                // Remove all old joined sessions
                foreach (string joinedSessionCode in GetJoinedSessionCodes(user))
                {
                    _logger.LogInformation("Removing old Session code: {0} user: {1}", joinedSessionCode, user);
                    _sessions.TryGetValue(joinedSessionCode, out Session joinedSession);
                    joinedSession.RemoveUser(user);
                }

                // Add user to the new session
                session.AddUser(user);
                session.TouchSession();
                _logger.LogInformation("Join Session user: {0}, code: {1}", user, sessionCode);
                return true;
            }

            return false;
        }

        private IEnumerable<string> GetJoinedSessionCodes(string user)
        {
            IEnumerable<string> sessionCodesJoinedByUser = from pair in _sessions
                                                           where pair.Value.Type == SessionTypeEnum.Active && pair.Value.HasUser(user)
                                                           select pair.Key;

            return sessionCodesJoinedByUser;
        }

        public void QuitSession(string sessionCode, string user)
        {
            bool sessionExists = _sessions.TryGetValue(sessionCode, out Session session);

            if (!sessionExists)
            {
                return;
            }
            session.RemoveUser(user);
            session.TouchSession();
            _logger.LogInformation("Quit Session user: {0}, code: {1}", user, sessionCode);
        }

        public SessionModel GenerateSessionModel(string sessionCode, string user)
        {
            bool success = _sessions.TryGetValue(sessionCode, out Session session);

            if (!success)
            {
                return null;
            }

            return new SessionModel(user, session.Creator, sessionCode, session.IsLocked, session.SavedState);
        }

        public void LockSession(string sessionCode)
        {
            bool success = _sessions.TryGetValue(sessionCode, out Session session);

            if (!success)
            {
                return;
            }

            session.Lock();
            session.TouchSession();
        }

        public void UnlockSession(string sessionCode)
        {
            bool success = _sessions.TryGetValue(sessionCode, out Session session);

            if (!success)
            {
                return;
            }

            session.Unlock();
            session.TouchSession();
        }

        public void UpdateSessionState(string sessionCode, string savedState)
        {
            bool success = _sessions.TryGetValue(sessionCode, out Session session);

            if (!success)
            {
                return;
            }

            session.SavedState = HttpUtility.UrlDecode(savedState);
            session.TouchSession();
        }

        public bool IsLegalUser(string sessionCode, string user)
        {
            return _sessions.TryGetValue(sessionCode, out Session _);
        }

        public bool IsLegalCreator(string sessionCode, string creator)
        {
            bool success = _sessions.TryGetValue(sessionCode, out Session session);

            if (!success)
            {
                return false;
            }

            return session.Creator == creator;
        }

        private void CheckSession()
        {
            List<string> expiredSessionCodes = new List<string>();

            foreach (var pair in _sessions)
            {
                Session session = pair.Value;
                if (session.Type == SessionTypeEnum.Active)
                {
                    var elapsed = DateTime.UtcNow - session.LastActiveDateTime;
                    if (elapsed > _sessionExpireThreshold) // Timeout
                    {
                        _logger.LogInformation("Session sessCode: {0} time out. Force expire. Creator: {1}", session.SessionCode, session.Creator);
                        session.Expire();
                        expiredSessionCodes.Add(pair.Key);
                    } else if (session.GetUsers().Length == 0) // Not timeout but has zero participants
                    {
                        _logger.LogInformation("Session sessCode: {0} no participants. Force expire. Creator: {1}", session.SessionCode, session.Creator);
                        session.Expire();
                        expiredSessionCodes.Add(pair.Key);
                    }
                }
            }

            foreach (string expiredSessionCode in expiredSessionCodes)
            {
                _sessions.Remove(expiredSessionCode, out Session expiredSession);
                expiredSession.Dispose();
            }
        }

        public void Dispose()
        {
            _sessionChecker.Dispose();
            _sessions.Clear();
        }
    }
}
