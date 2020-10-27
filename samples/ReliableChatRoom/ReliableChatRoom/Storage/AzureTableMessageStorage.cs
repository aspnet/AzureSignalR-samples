using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public class AzureTableMessageStorage : IMessageStorage
    {
        private readonly IMessageFactory _messageFactory;
        private readonly IHubContext<ReliableChatRoomHub> _hubContext;
        private readonly CloudStorageAccount _cloudStorageAccount;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudTable _cloudTable;
        private readonly string _tableName = "mobilechatroom";
        private readonly string _dateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffff";

        public AzureTableMessageStorage(IHubContext<ReliableChatRoomHub> hubContext, IMessageFactory messageFactory, string connectionString)
        {
            _hubContext = hubContext;
            _messageFactory = messageFactory;
            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _cloudTable = _cloudTableClient.GetTableReference(_tableName);
        }

        public async Task<bool> TryStoreMessageAsync(Message message, OnStoreSuccess callback)
        {
            try
            {
                MessageEntity messageEntity = CreateMessageEntity(message);
                await _cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(messageEntity));
            } catch (Exception ex) // Any failure in ExecuteAsync will appear as exception
            {
                Console.WriteLine(ex.Message);

                // Directly return and do not execute the callback method
                return false;
            }
            
            // Callback
            return await callback(message, _hubContext);
        }

        public async Task<bool> FetchHistoryMessageAsync(string username, DateTime endDateTime, OnFetchSuccess callback)
        {
            string endDateTimeString = endDateTime.ToString(_dateFormatString);

            // Define Linq query
            TableQuery<MessageEntity> messageQuery = _cloudTable.CreateQuery<MessageEntity>();
            var query = (from message in messageQuery
                         where message.RowKey.CompareTo(endDateTimeString) < 0 &&
                         (message.Sender.CompareTo(username) == 0 ||
                         message.Receiver.CompareTo(username) == 0)
                         select message).Take(5).AsTableQuery();

            // Execute query
            TableQuerySegment<MessageEntity> messageEntities;
            try
            {
                messageEntities = await query.ExecuteSegmentedAsync(new TableContinuationToken());
            } catch (Exception ex) // Any failure in ExecuteSegmentedAsync will appear as exception
            {
                Console.WriteLine(ex.Message);

                // Directly return and do not execute the callback method
                return false;
            }

            // Process query result
            List<Message> historyMessages = new List<Message>();
            foreach (var messageEntity in messageEntities)
            {
                historyMessages.Add(_messageFactory.FromSingleJsonString(messageEntity.MessageJsonString));
            }

            //  Callback
            return await callback(historyMessages, _hubContext);
        }

        private MessageEntity CreateMessageEntity(Message message)
        {
            return new MessageEntity(message.MessageId, message.SendTime.ToString(_dateFormatString), message.Sender, message.Receiver, _messageFactory.ToSingleJsonString(message));
        }

        public class MessageEntity : TableEntity
        {
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string MessageJsonString { get; set; }

            public MessageEntity()
            {

            }

            public MessageEntity(string messageId, string dateTimeString, string sender, string receiver, string messageJsonString)
            {
                PartitionKey = messageId;
                RowKey = dateTimeString;
                Sender = sender;
                Receiver = receiver;
                MessageJsonString = messageJsonString;
            }

        }
    }
}
