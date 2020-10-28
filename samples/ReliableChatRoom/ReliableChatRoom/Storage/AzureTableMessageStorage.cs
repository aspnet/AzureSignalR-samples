using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    public class AzureTableMessageStorage : IMessageStorage
    {
        private readonly IMessageFactory _messageFactory;

        private readonly CloudStorageAccount _cloudStorageAccount;
        private readonly CloudTableClient _cloudTableClient;
        private readonly CloudTable _cloudTable;
        private readonly string _tableName = "mobilechatroom";

        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _containerName = "mobilechatroom";

        private readonly string _dateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffff";
        private readonly int _messageCountPerFetch = 5;

        public AzureTableMessageStorage(IMessageFactory messageFactory, string connectionString)
        {
            _messageFactory = messageFactory;

            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _cloudTable = _cloudTableClient.GetTableReference(_tableName);

            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<bool> TryStoreMessageAsync(Message message)
        {
            try
            {
                MessageEntity messageEntity = CreateMessageEntity(message);
                List<Task> tasks = new List<Task>();

                tasks.Add(_cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(messageEntity)));

                if (message.IsImage)
                {
                    tasks.Add(TryStoreImageAsync(message.MessageId, message.ImagePayload));
                }

                await Task.WhenAll(tasks);
            } catch (Exception ex) // Any failure in ExecuteAsync will appear as exception
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        private async Task<bool> TryStoreImageAsync(string messageId, string imagePayload)
        {
            using (var stream = GenerateStreamFromString(imagePayload))
            {
                await _blobContainerClient.UploadBlobAsync(messageId, stream);
            }

            return true;
        }

        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        public async Task<bool> TryFetchHistoryMessageAsync(string username, DateTime endDateTime, List<Message> historyMessages)
        {
            string endDateTimeString = endDateTime.ToString(_dateFormatString);

            // Define Linq query
            TableQuery<MessageEntity> messageQuery = _cloudTable.CreateQuery<MessageEntity>();
            var query = (from message in messageQuery
                         where message.RowKey.CompareTo(endDateTimeString) < 0 &&
                         (message.Sender.CompareTo(username) == 0 ||
                         message.Receiver.CompareTo(username) == 0)
                         select message).AsTableQuery();

            List<MessageEntity> messageEntities;
            try
            {
                // Execute query
                TableQuerySegment<MessageEntity> messageEntitiesQuerySegment;
                messageEntitiesQuerySegment = await query.ExecuteSegmentedAsync(new TableContinuationToken());

                // Sort by time desc
                messageEntities = messageEntitiesQuerySegment.ToList();
                messageEntities.Sort((p, q) => (string.Compare(q.RowKey, p.RowKey)));
            } catch (Exception ex) // Any failure in ExecuteSegmentedAsync will appear as exception
            {
                Console.WriteLine(ex.Message);

                // Load failed
                return false;
            }

            // Process query result with a limit of 
            foreach (var messageEntity in messageEntities.Take(_messageCountPerFetch))
            {
                historyMessages.Add(_messageFactory.FromSingleJsonString(messageEntity.MessageJsonString));
            }

            return true;
        }

        public async Task<string> TryFetchImageContent(string messageId)
        {
            var blobClient = _blobContainerClient.GetBlobClient(messageId);

            //  Download to a stream
            Stream downloadedStream = (await blobClient.DownloadAsync()).Value.Content;

            //  Read the stream into a jsonString
            StreamReader streamReader = new StreamReader(downloadedStream);
            string imagePayload = streamReader.ReadToEnd();

            return imagePayload;
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
