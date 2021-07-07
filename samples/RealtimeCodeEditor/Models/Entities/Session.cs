using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeCodeEditor.Models.Entities
{
    public class Session : IDisposable
    {
        public string SessionCode { get; private set; }

        public SessionTypeEnum Type { get; set; }

        public string Creator { get; private set; }

        public bool IsLocked { get; private set; }

        public string SavedState { get; set; }

        public DateTime LastActiveDateTime { get; set; }

        private ISet<string> _users;

        public Session(string sessionCode, string creator)
        {
            SessionCode = sessionCode;
            Type = SessionTypeEnum.Active;
            Creator = creator;
            IsLocked = false;
            _users = new HashSet<string>();
        }

        public void AddUser(string user)
        {
            if (Type == SessionTypeEnum.Expired)
            {
                throw new Exception("Attempts to update an expired session.");
            }

            _users.Add(user);
        }

        public void RemoveUser(string user)
        {
            if (Type == SessionTypeEnum.Expired)
            {
                throw new Exception("Attempts to update an expired session.");
            }

            _users.Remove(user);
        }

        public string[] GetUsers(string except = "")
        {
            if (except == "")
            {
                return _users.ToArray();
            }
            else
            {
                return _users.Where(s => s != except).ToArray();
            }
        }

        public bool HasUser(string user)
        {
            return _users.Contains<string>(user);
        }

        public void TouchSession()
        {
            if (Type == SessionTypeEnum.Expired)
            {
                throw new Exception("Attempts to touch an expired session.");
            }

            LastActiveDateTime = DateTime.UtcNow;
        }

        public void Expire()
        {
            Type = SessionTypeEnum.Expired;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }

        public void Dispose()
        {
            _users.Clear();
        }

    }
}
