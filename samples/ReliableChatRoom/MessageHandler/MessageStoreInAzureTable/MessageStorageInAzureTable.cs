using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class MessageStorageInAzureTable : IMessageHandler
    {
        private readonly CloudStorageAccount _storageAccount;

        private readonly CloudTableClient _cloudTableClient;

        private readonly CloudTable _messageTable;

        private readonly IConfiguration _configurationuration;

        public MessageStorageInAzureTable(IConfiguration configuration)
        {
            _configurationuration = configuration;
            _storageAccount = CloudStorageAccount.Parse(_configurationuration.GetConnectionString("AzureStorage"));

            _cloudTableClient = _storageAccount.CreateCloudTableClient();

            _messageTable = _cloudTableClient.GetTableReference("MessageTable");
            _messageTable.CreateIfNotExistsAsync();
        }

        public async Task<int> AddNewMessageAsync(string sessionId, Message message)
        {
            var messageTime = DateTime.Now.Ticks;
            var messageEntity = new MessageEntity(sessionId, messageTime.ToString(), message);

            TableOperation insertOperation = TableOperation.Insert(messageEntity);
            var task = await _messageTable.ExecuteAsync(insertOperation);

            return (int)messageTime;
        }

        public async Task UpdateMessageAsync(string sessionId, int sequenceId, string messageStatus)
        {
            var retrieveOperation = TableOperation.Retrieve<MessageEntity>(sessionId, sequenceId.ToString());
            var retrievedResult = await _messageTable.ExecuteAsync(retrieveOperation);
            var updateEntity = retrievedResult.Result as MessageEntity;


            if (updateEntity != null)
            {
                var message = updateEntity.ToMessage();
                message.MessageStatus = messageStatus;
                updateEntity.Message = JsonConvert.SerializeObject(message);

                var updateOperation = TableOperation.Replace(updateEntity);
                await _messageTable.ExecuteAsync(updateOperation);
            }
        }

        public async Task<List<Message>> LoadHistoryMessageAsync(string sessionId, int startSequenceId, int endSequenceId)
        {
            // TODO: Select the messages by 2 sequenceId params
            var query = new TableQuery<MessageEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, sessionId)
            );
            var result = await _messageTable.ExecuteQuerySegmentedAsync(query, null);

            var messages = new List<Message>();
            foreach(var entity in result)
            {
                messages.Add(entity.ToMessage());
            }

            return messages;
        }
    }
}
