using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class MessageEntity : TableEntity
    {
        public string Message { get; set; }

        public MessageEntity() { }

        public MessageEntity(string pkey, string rkey)
        {
            PartitionKey = pkey;
            RowKey = rkey;
        }

        public MessageEntity(string pkey, string rkey, Message message)
        {
            PartitionKey = pkey;
            RowKey = rkey;
            Message = JsonConvert.SerializeObject(message);
        }

        public Message ToMessage()
        {
            return JsonConvert.DeserializeObject<Message>(Message);
        }
    }
}
