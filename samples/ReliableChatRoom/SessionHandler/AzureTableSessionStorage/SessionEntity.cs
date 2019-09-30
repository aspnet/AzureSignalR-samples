// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage.Table;

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
