using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SignalR.Sample
{
    public static class Functions
    {
        private const string TableName = "connection";
        private const string EventGridConnectedEventName = "Microsoft.SignalRService.ClientConnectionConnected";
        private const string HubName = "EventGridIntegrationSampleChat";

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = HubName)] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
            [SignalR(HubName = HubName)]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));
            message.Sender = req.Headers?["x-ms-client-principal-name"] ?? "";
            var recipientUserId = "";
            if (!string.IsNullOrEmpty(message.Recipient))
            {
                recipientUserId = message.Recipient;
                message.IsPrivate = true;
            }

            return signalRMessages.AddAsync(new SignalRMessage
            {
                UserId = message.Recipient,
                Target = "newMessage",
                Arguments = new[] { message }
            });
        }

        [FunctionName("OnConnection")]
        public static async Task EventGridTest([EventGridTrigger]EventGridEvent eventGridEvent,
            [SignalR(HubName = HubName)]IAsyncCollector<SignalRMessage> signalRMessages,
            [Table(TableName)]CloudTable cloudTable,
            ILogger log)
        {
            var message = JsonConvert.DeserializeObject<SignalREvent>(eventGridEvent.Data.ToString());
            var partitionKey = GetLastPart(eventGridEvent.Topic);
            var rowKey = message.HubName;
            var isSuccess = true;
            var newConnectionCount = 0;
            
            while (isSuccess)
            {
                try
                {
                    ConnectionCountEntity entity;
                    var operation = TableOperation.Retrieve<ConnectionCountEntity>(partitionKey, rowKey);
                    var result = await cloudTable.ExecuteAsync(operation);

                    if (result.Result == null)
                    {
                        entity = new ConnectionCountEntity(partitionKey, rowKey)
                        {
                            Count = newConnectionCount = IsConnectedEvent(eventGridEvent.EventType) ? 1 : 0
                        };
                        operation = TableOperation.Insert(entity);
                    }
                    else
                    {
                        entity = (ConnectionCountEntity)result.Result;
                        entity.Count = newConnectionCount = entity.Count + (IsConnectedEvent(eventGridEvent.EventType) ? 1 : -1);
                        operation = TableOperation.Replace(entity);
                    }

                    await cloudTable.ExecuteAsync(operation);
                    isSuccess = false;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to complete operation with storage");
                }
            }

            if (IsConnectedEvent(eventGridEvent.EventType))
            {
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    ConnectionId = message.ConnectionId,
                    Target = "newMessage",
                    Arguments = new[] { new ChatMessage
                    {
                        Text = "Welcome to Serverless Chat",
                        Sender = "__SYSTEM__",
                    }}
                });
            }

            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = "connectionCount",
                Arguments = new[] { (object)newConnectionCount },
            });
        }

        private static string GetLastPart(string data)
        {
            var index = data.LastIndexOf('/');
            if (index == -1)
            {
                return data;
            }
            else
            {
                return data.Substring(index + 1);
            }
        }

        private static bool IsConnectedEvent(string name) => name == EventGridConnectedEventName;

        public class ChatMessage
        {
            [JsonProperty("sender")]
            public string Sender { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("recipient")]
            public string Recipient { get; set; }
            [JsonProperty("isPrivate")]
            public bool IsPrivate { get; set; }
        }

        public class SignalREvent
        {
            public DateTime Timestamp { get; set; }
            public string HubName { get; set; }
            public string ConnectionId { get; set; }
            public string UserId { get; set; }
        }

        public class ConnectionCountEntity : TableEntity
        {
            public int Count { get; set; }

            public ConnectionCountEntity()
            {
            }

            public ConnectionCountEntity(string partitionKey, string rowKey)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }
        }
    }
}
