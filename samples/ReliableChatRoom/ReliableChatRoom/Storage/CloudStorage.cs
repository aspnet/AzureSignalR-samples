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
    public class CloudStorage : IPersistentStorage
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly IMessageFactory _messageFactory;

        private readonly DateTime _defaultDateTime = new DateTime(1970, 1, 1);
        private readonly string _containerName = "mobilechatroom";

        public CloudStorage(IMessageFactory messageFactory, string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            _messageFactory = messageFactory;
        }

        public bool TryLoadMessageList(DateTime startDateTime, DateTime endDateTime, out List<Message> outMessages)
        {
            // Blob names
            List<string> blobNames = GetDescBlobNames(_blobContainerClient);

            // History messages
            outMessages = new List<Message>();
            foreach (string blobName in blobNames)
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobName);
                outMessages.AddRange(GetMessagesFromBlobOnPredicate(blobClient, (m) => (m.SendTime >= startDateTime && m.SendTime < endDateTime)));
            }

            return true;
        }

        public bool TryStoreMessageList(List<Message> messages)
        {
            string blobName = Guid.NewGuid().ToString();
            string jsonString = _messageFactory.ToJsonString(messages);
            using (var stream = GenerateStreamFromString(jsonString))
            {
                _blobContainerClient.UploadBlob(blobName, stream);
            }
            
            return true;
        }

        private List<string> GetDescBlobNames(BlobContainerClient blobContainerClient)
        {
            // Get blob names inside that container
            List<string> blobNames = new List<string>();
            foreach (BlobItem blobItem in blobContainerClient.GetBlobs())
            {
                blobNames.Add(blobItem.Name);
            }

            // Search for messages from end to start
            blobNames.Sort((p, q) => q.CompareTo(p));
            
            return blobNames;
        }

        private List<Message> GetMessagesFromBlobOnPredicate(BlobClient blobClient, Func<Message, bool> func)
        {
            StreamReader streamReader = new StreamReader(blobClient.Download().Value.Content);
            string jsonString = streamReader.ReadToEnd();

            List<Message> storedMessages = _messageFactory.FromJsonString(jsonString);

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
