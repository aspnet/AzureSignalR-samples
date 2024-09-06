using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimeCodeEditor.Models
{
    public class SessionModel
    {
        public string User { get; set; }

        public string Creator { get; set; }

        public string SessionCode { get; set; }

        public bool IsLocked { get; set; }

        public string SavedState { get; set; }

        public SessionModel(string user, string creator, string sessionCode, bool isLocked, string savedState)
        {
            User = user;
            Creator = creator;
            SessionCode = sessionCode;
            IsLocked = isLocked;
            SavedState = savedState;
        }
    }
}
