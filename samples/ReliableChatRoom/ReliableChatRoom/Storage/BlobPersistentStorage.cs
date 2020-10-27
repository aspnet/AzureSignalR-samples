using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Storage
{
    [Obsolete]
    public class BlobPersistentStorage : IPersistentStorage
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;

        private readonly IMessageFactory _messageFactory;

        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);
        private readonly string _containerName = "mobilechatroom";

        public BlobPersistentStorage(IMessageFactory messageFactory, string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            _messageFactory = messageFactory;
        }

        public async Task<List<Message>> TryLoadMessageListAsync(DateTime startDateTime, DateTime endDateTime)
        {
            // Blob names
            List<string> blobNames = await GetDescBlobNames(_blobContainerClient);

            // History messages
            List<Message> historyMessages = new List<Message>();
            foreach (string blobName in blobNames)
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobName);
                historyMessages.AddRange(await GetMessagesFromBlobOnPredicate(blobClient, (m) => (m.SendTime >= startDateTime && m.SendTime < endDateTime)));
            }

            return historyMessages;
        }

        public async Task<bool> TryStoreMessageListAsync(List<Message> messages)
        {
            string blobName = Guid.NewGuid().ToString();
            string jsonString = _messageFactory.ToListJsonString(messages);
            using (var stream = GenerateStreamFromString(jsonString))
            {
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
            
            return true;
        }

        private async Task<List<string>> GetDescBlobNames(BlobContainerClient blobContainerClient)
        {
            // Get blob names inside that container
            List<string> blobNames = new List<string>();
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }

            // Search for messages from end to start
            blobNames.Sort((p, q) => q.CompareTo(p));
            
            return blobNames;
        }

        private async Task<List<Message>> GetMessagesFromBlobOnPredicate(BlobClient blobClient, Func<Message, bool> func)
        {
            //  Download to a stream
            Stream downloadedStream = (await blobClient.DownloadAsync()).Value.Content;
            
            //  Read the stream into a jsonString
            StreamReader streamReader = new StreamReader(downloadedStream);
            string jsonString = streamReader.ReadToEnd();

            //  Convert the jsonString to list of Messages
            List<Message> storedMessages = _messageFactory.FromListJsonString(jsonString);

            return storedMessages.Where(func).ToList();
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
    }
}
