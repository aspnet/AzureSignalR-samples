using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class UserHandler : IUserHandler
    {
        private readonly ConcurrentDictionary<string, (string, string)> _loginTable =
            new ConcurrentDictionary<string, (string, string)>();

        public string GetUserConnectionId(string username)
        {
            _loginTable.TryGetValue(username, out (string, string) pair);
            return pair.Item1;
        }

        public string GetUserDeviceToken(string username)
        {
            _loginTable.TryGetValue(username, out (string, string) pair);
            return pair.Item2;
        }

        public (string, string) Login(string username, string connectionId, string deviceToken)
        {
            _loginTable.AddOrUpdate(username, (connectionId, deviceToken), (v1, v2)=>(connectionId, deviceToken));
            bool isSuccess = _loginTable.TryGetValue(username, out (string, string) value);
            if (isSuccess)
            {
                return value;
            }
            return (null, null);
        }

        public void Logout(string username)
        {
            _loginTable.TryRemove(username, out _);
        }

    }
}
