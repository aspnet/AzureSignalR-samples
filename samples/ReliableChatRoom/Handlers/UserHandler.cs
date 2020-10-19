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
        private readonly ConcurrentDictionary<string, (string, string, DateTime)> _userTable =
            new ConcurrentDictionary<string, (string, string, DateTime)>();
        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);

        private readonly Timer _sessionCheckingTimer;
        private readonly TimeSpan _sessionExpireThreshold = TimeSpan.FromSeconds(60);

        public UserHandler()
        {

        }

        public string GetUserConnectionId(string username)
        {
            _userTable.TryGetValue(username, out (string, string, DateTime) pair);
            return pair.Item1;
        }

        public string GetUserDeviceToken(string username)
        {
            _userTable.TryGetValue(username, out (string, string, DateTime) pair);
            return pair.Item2;
        }

        public DateTime GetUserLastTouch(string username)
        {
            _userTable.TryGetValue(username, out (string, string, DateTime) pair);
            return pair.Item3;
        }

        public (string, string) Login(string username, string connectionId, string deviceToken)
        {
            _userTable.AddOrUpdate(username, (connectionId, deviceToken, DateTime.UtcNow), (k, v) => (connectionId, deviceToken, DateTime.UtcNow));
            bool isSuccess = _userTable.TryGetValue(username, out (string, string, DateTime) value);
            if (isSuccess)
            {
                return (value.Item1, value.Item2);
            }
            return (null, null);
        }

        public DateTime Touch(string username, string connectionId, string deviceToken)
        {
            if (!_userTable.ContainsKey(username))
            {
                return _defaultDateTime;
            }

            var oldVal = _userTable[username];
            if (!connectionId.Equals(oldVal.Item1))
            {
                Console.WriteLine(string.Format("Touch username: {0}\nconnectionId old: {1}\nconnectionId new: {2}", username, oldVal.Item1, connectionId));
            }

            _userTable.TryUpdate(username, (connectionId, deviceToken, DateTime.UtcNow), oldVal);
            _userTable.TryGetValue(username, out (string, string, DateTime) value);
            
            return value.Item3;
        }

        public string Logout(string connectionId)
        {
            string username = "";
            foreach (var pair in _userTable)
            {
                username = pair.Key;
                if (pair.Value.Item1.Equals(connectionId))
                {
                    break;
                }
            }
            _userTable.TryRemove(username, out _);
            return username;
        }

        private void CheckSession()
        {
            foreach (var pair in _userTable) {
                string username = pair.Key;
                var elapsed = DateTime.UtcNow - GetUserLastTouch(username);
                if (elapsed > _checkAckThreshold)
                {
                    Console.WriteLine(string.Format("Ack id: {0} time out", clientAck.ClientAckId));
                    clientAck.TimeOut();
                }
                
            }
        }
    }
}
