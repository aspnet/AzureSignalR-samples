using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace IsolatedModel_BidirectionChat
{
    public class Functions
    {
        private readonly ILogger _logger;

        public Functions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Functions>();
        }

        [Function("index")]
        public HttpResponseData GetWebPage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(File.ReadAllText("content/index.html"));
            response.Headers.Add("Content-Type", "text/html");
            return response;
        }

        [Function("Negotiate")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "Hub", UserId = "{query.userid}")] SignalRConnectionInfo signalRConnectionInfo)
        {
            _logger.LogInformation("Executing negotiation.");
            return signalRConnectionInfo;
        }

        [Function("OnConnected")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction OnConnected([SignalRTrigger("Hub", "connections", "connected")] SignalRInvocationContext invocationContext)
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            _logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            return new SignalRMessageAction("newConnection")
            {
                Arguments = new object[] { new NewConnection(invocationContext.ConnectionId, auth) },

            };
        }

        [Function("Broadcast")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction Broadcast([SignalRTrigger("Hub", "messages", "Broadcast", "message")] SignalRInvocationContext invocationContext, string message)
        {
            return new SignalRMessageAction("newMessage")
            {
                Arguments = new object[] { new NewMessage(invocationContext, message) }
            };
        }

        [Function("SendToGroup")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction SendToGroup([SignalRTrigger("Hub", "messages", "SendToGroup", "groupName", "message")] SignalRInvocationContext invocationContext, string groupName, string message)
        {
            return new SignalRMessageAction("newMessage")
            {
                GroupName = groupName,
                Arguments = new object[] { new NewMessage(invocationContext, message) }
            };
        }

        [Function("SendToUser")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction SendToUser([SignalRTrigger("Hub", "messages", "SendToUser", "userName", "message")] SignalRInvocationContext invocationContext, string userName, string message)
        {
            return new SignalRMessageAction("newMessage")
            {
                UserId = userName,
                Arguments = new object[] { new NewMessage(invocationContext, message) }
            };
        }

        [Function("SendToConnection")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction SendToConnection([SignalRTrigger("Hub", "messages", "SendToConnection", "connectionId", "message")] SignalRInvocationContext invocationContext, string connectionId, string message)
        {
            return new SignalRMessageAction("newMessage")
            {
                ConnectionId = connectionId,
                Arguments = new object[] { new NewMessage(invocationContext, message) }
            };
        }

        [Function("JoinGroup")]
        [SignalROutput(HubName = "Hub")]
        public SignalRGroupAction JoinGroup([SignalRTrigger("Hub", "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                GroupName = groupName,
                ConnectionId = connectionId
            };
        }

        [Function("LeaveGroup")]
        [SignalROutput(HubName = "Hub")]
        public SignalRGroupAction LeaveGroup([SignalRTrigger("Hub", "messages", "LeaveGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return new SignalRGroupAction(SignalRGroupActionType.Remove)
            {
                GroupName = groupName,
                ConnectionId = connectionId
            };
        }

        [Function("JoinUserToGroup")]
        [SignalROutput(HubName = "Hub")]
        public SignalRGroupAction JoinUserToGroup([SignalRTrigger("Hub", "messages", "JoinUserToGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                GroupName = groupName,
                UserId = userName
            };
        }

        [Function("LeaveUserFromGroup")]
        [SignalROutput(HubName = "Hub")]
        public SignalRGroupAction LeaveUserFromGroup([SignalRTrigger("Hub", "messages", "LeaveUserFromGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return new SignalRGroupAction(SignalRGroupActionType.Remove)
            {
                GroupName = groupName,
                UserId = userName
            };
        }

        [Function("OnDisconnected")]
        [SignalROutput(HubName = "Hub")]
        public void OnDisconnected([SignalRTrigger("Hub", "connections", "disconnected")] SignalRInvocationContext invocationContext)
        {
            _logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
        }

        public class NewConnection
        {
            public string ConnectionId { get; }

            public string Authentication { get; }

            public NewConnection(string connectionId, string auth)
            {
                ConnectionId = connectionId;
                Authentication = auth;
            }
        }

        public class NewMessage
        {
            public string ConnectionId { get; }
            public string Sender { get; }
            public string Text { get; }

            public NewMessage(SignalRInvocationContext invocationContext, string message)
            {
                Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
                ConnectionId = invocationContext.ConnectionId;
                Text = message;
            }
        }
    }
}
