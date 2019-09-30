// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class AzureTableSessionStorage : ISessionHandler
    {
        private readonly CloudStorageAccount _storageAccount;

        private readonly CloudTableClient _cloudTableClient;

        private readonly CloudTable _sessionTable;

        private readonly IConfiguration _configuration;

        public AzureTableSessionStorage(IConfiguration configuration)
        {
            _configuration = configuration;
            _storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("AzureStorage"));

            _cloudTableClient = _storageAccount.CreateCloudTableClient();

            _sessionTable = _cloudTableClient.GetTableReference("SessionTable");
            _sessionTable.CreateIfNotExistsAsync();
        }

        public async Task<Session> GetOrCreateSessionAsync(string userName, string partnerName)
        {
            var retrieveOperation = TableOperation.Retrieve<SessionEntity>(userName, partnerName);
            var retrievedResult = await _sessionTable.ExecuteAsync(retrieveOperation);
            var sessionEntity = retrievedResult.Result as SessionEntity;

            if (sessionEntity != null) {
                return sessionEntity.ToSession();
            }

            var session = new Session(Guid.NewGuid().ToString());
            await _sessionTable.ExecuteAsync(TableOperation.Insert(new SessionEntity(userName, partnerName, session)));
            await _sessionTable.ExecuteAsync(TableOperation.Insert(new SessionEntity(partnerName, userName, session)));

            return session;
        }

        public async Task<KeyValuePair<string, Session>[]> GetLatestSessionsAsync(string userName)
        {
            var query = new TableQuery<SessionEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userName)
            );
            var result = await _sessionTable.ExecuteQuerySegmentedAsync<SessionEntity>(query, null);

            var sessions = new SortedDictionary<string, Session>();
            foreach(var entity in result)
            {
                sessions.Add(entity.RowKey, entity.ToSession());
            }

            return sessions.ToArray();
        }
    }
}
