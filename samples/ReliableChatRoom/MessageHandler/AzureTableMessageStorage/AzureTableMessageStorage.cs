// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class AzureTableMessageStorage : IMessageHandler
    {
        private readonly CloudStorageAccount _storageAccount;

        private readonly CloudTableClient _cloudTableClient;

        private readonly CloudTable _messageTable;

        private readonly IConfiguration _configurationuration;

        public AzureTableMessageStorage(IConfiguration configuration)
        {
            _configurationuration = configuration;
            _storageAccount = CloudStorageAccount.Parse(_configurationuration.GetConnectionString("AzureStorage"));

            _cloudTableClient = _storageAccount.CreateCloudTableClient();

            _messageTable = _cloudTableClient.GetTableReference("MessageTable");
            _messageTable.CreateIfNotExistsAsync();
        }

        public async Task<string> AddNewMessageAsync(string sessionId, Message message)
        {
            var messageTime = DateTime.Now.Ticks.ToString();
            var messageEntity = new MessageEntity(sessionId, messageTime, message);

            TableOperation insertOperation = TableOperation.Insert(messageEntity);
            var task = await _messageTable.ExecuteAsync(insertOperation);

            return messageTime;
        }

        public async Task UpdateMessageAsync(string sessionId, string sequenceId, string messageStatus)
        {
            var retry = 0;
            const int MAX_RETRY = 10;

            while (retry < MAX_RETRY)
            {
                try
                {
                    var retrieveOperation = TableOperation.Retrieve<MessageEntity>(sessionId, sequenceId);
                    var retrievedResult = await _messageTable.ExecuteAsync(retrieveOperation);
                    var updateEntity = retrievedResult.Result as MessageEntity;

                    if (updateEntity != null)
                    {
                        updateEntity.UpdateMessageStatus(messageStatus);

                        var updateOperation = TableOperation.Replace(updateEntity);
                        await _messageTable.ExecuteAsync(updateOperation);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    if (++retry == MAX_RETRY) 
                    {
                        throw ex;
                    }

                    await Task.Delay(new Random().Next(10, 100));
                }
            }
        }

        public async Task<List<Message>> LoadHistoryMessageAsync(string sessionId)
        {
            var query = new TableQuery<MessageEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, sessionId)
            );
            var result = await _messageTable.ExecuteQuerySegmentedAsync(query, null);

            var messages = new List<Message>();
            foreach(var entity in result)
            {
                var message = entity.ToMessage();
                message.SequenceId = entity.RowKey;
                messages.Add(message);
            }

            return messages;
        }
    }
}
