using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public interface IUserHandler
    {
        Session Login(string username, string connectionId, string deviceToken);
        DateTime Touch(string username, string connectionId, string deviceToken);
        Session Logout(string connectionId);
        Session GetUserSession(string username);
    }
}
