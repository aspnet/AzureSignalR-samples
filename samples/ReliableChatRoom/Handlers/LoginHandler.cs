﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Handlers
{
    public class LoginHandler : ILoginHandler
    {
        private readonly ConcurrentDictionary<string, (string, string)> _loginTable =
            new ConcurrentDictionary<string, (string, string)>();


        public (string, string) Login(string username, string connectionId, string deviceToken)
        {
            _loginTable.AddOrUpdate(username, (connectionId, deviceToken), (v1, v2)=>(connectionId, deviceToken));
            _loginTable.TryGetValue(username, out (string, string) value);
            return value;
        }

        public void Logout(string username)
        {
            _loginTable.TryRemove(username, out _);
        }

    }
}
