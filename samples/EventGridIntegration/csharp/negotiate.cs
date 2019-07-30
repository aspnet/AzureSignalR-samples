using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SignalR.Sample
{
    public static class Program
    {
        private const string TableName = "connection";

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "EventGridIntegrationSampleChat")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
            [SignalR(HubName = "EventGridIntegrationSampleChat")]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));
            message.sender = req.Headers?["x-ms-client-principal-name"] ?? "";

            var recipientUserId = "";
            if (!string.IsNullOrEmpty(message.recipient))
            {
                recipientUserId = message.recipient;
                message.isPrivate = true;
            }

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = message.recipient,
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
        }

        [FunctionName("OnConnection")]
        public static async Task EventGridTest([EventGridTrigger]EventGridEvent eventGridEvent,
            [SignalR(HubName = "EventGridIntegrationSampleChat")]IAsyncCollector<SignalRMessage> signalRMessages,
            [Table(TableName)]CloudTable cloudTable,
            ILogger log)
        {
            var message = ((JObject) eventGridEvent.Data).ToObject<SignalREvent>();
            var partitionKey = GetLastPart(eventGridEvent.Topic);
            var rowKey = message.HubName;
            var token = true;
            var newConnectionCount = 0;
            ConnectionCountEntity entity;
            while (token)
            {
                try
                {
                    var operation = TableOperation.Retrieve<ConnectionCountEntity>(partitionKey, rowKey);
                    var result = await cloudTable.ExecuteAsync(operation);

                    if (result.Result == null)
                    {
                        entity = new ConnectionCountEntity(partitionKey, rowKey)
                        {
                            Count = newConnectionCount = eventGridEvent.EventType == "Microsoft.SignalRService.ClientConnectionConnected" ? 1 : 0
                        };
                        operation = TableOperation.Insert(entity);
                    }
                    else
                    {
                        entity = (ConnectionCountEntity)result.Result;
                        entity.Count = newConnectionCount = entity.Count + (eventGridEvent.EventType == "Microsoft.SignalRService.ClientConnectionConnected" ? 1 : -1);
                        operation = TableOperation.Replace(entity);
                    }

                    await cloudTable.ExecuteAsync(operation);
                    token = false;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to complete operation with storage");
                }
            }

            if (eventGridEvent.EventType == "Microsoft.SignalRService.ClientConnectionConnected")
            {
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    ConnectionId = message.ConnectionId,
                    Target = "newMessage",
                    Arguments = new[] { new ChatMessage
                    {
                        text = "Welcome to Serverless Chat",
                        sender = "__SYSTEM__",
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

        public class ChatMessage
        {
            public string sender { get; set; }
            public string text { get; set; }
            public string recipient { get; set; }
            public bool isPrivate { get; set; }
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
