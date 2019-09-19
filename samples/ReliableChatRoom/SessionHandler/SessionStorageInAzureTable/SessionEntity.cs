using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class SessionEntity : TableEntity
    {
        public string SessionId { get; set; }

        public SessionEntity() { }

        public SessionEntity(string pkey, string rkey)
        {
            PartitionKey = pkey;
            RowKey = rkey;
        }

        public SessionEntity(string pkey, string rkey, Session session)
        {
            PartitionKey = pkey;
            RowKey = rkey;
            SessionId = session.SessionId;
        }

        public Session ToSession()
        {
            return new Session(SessionId);
        }
    }
}
