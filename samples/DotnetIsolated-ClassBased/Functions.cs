using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;

namespace DotnetIsolated_ClassBased
{
    [SignalRConnection("AzureSignalRConnectionString")]
    public class Functions : ServerlessHub<IChatClient>
    {
        private const string HubName = nameof(Functions);
        private readonly ILogger _logger;

        public Functions(IServiceProvider serviceProvider, ILogger<Functions> logger) : base(serviceProvider)
        {
            _logger = logger;
        }

        [Function("index")]
        public HttpResponseData GetWebPage([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(File.ReadAllText("content/index.html"));
            response.Headers.Add("Content-Type", "text/html");
            return response;
        }

        [Function("negotiate")]
        public async Task<HttpResponseData> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var negotiateResponse = await NegotiateAsync(new() { UserId = req.Headers.GetValues("userId").FirstOrDefault() });
            var response = req.CreateResponse();
            response.WriteBytes(negotiateResponse.ToArray());
            return response;
        }

        [Function("OnConnected")]
        public Task OnConnected([SignalRTrigger(HubName, "connections", "connected")] SignalRInvocationContext invocationContext)
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            _logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            return Clients.All.newConnection(new NewConnection(invocationContext.ConnectionId, auth));
        }

        [Function("Broadcast")]
        public Task Broadcast(
        [SignalRTrigger(HubName, "messages", "broadcast", "message")] SignalRInvocationContext invocationContext, string message)
        {
            return Clients.All.newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToGroup")]
        public Task SendToGroup([SignalRTrigger(HubName, "messages", "SendToGroup", "groupName", "message")] SignalRInvocationContext invocationContext, string groupName, string message)
        {
            return Clients.Group(groupName).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToUser")]
        public Task SendToUser([SignalRTrigger(HubName, "messages", "SendToUser", "userName", "message")] SignalRInvocationContext invocationContext, string userName, string message)
        {
            return Clients.User(userName).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToConnection")]
        public Task SendToConnection([SignalRTrigger(HubName, "messages", "SendToConnection", "connectionId", "message")] SignalRInvocationContext invocationContext, string connectionId, string message)
        {
            return Clients.Client(connectionId).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("JoinGroup")]
        public Task JoinGroup([SignalRTrigger(HubName, "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return Groups.AddToGroupAsync(connectionId, groupName);
        }

        [Function("LeaveGroup")]
        public Task LeaveGroup([SignalRTrigger(HubName, "messages", "LeaveGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        [Function("JoinUserToGroup")]
        public Task JoinUserToGroup([SignalRTrigger(HubName, "messages", "JoinUserToGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return UserGroups.AddToGroupAsync(userName, groupName);
        }

        [Function("LeaveUserFromGroup")]
        public Task LeaveUserFromGroup([SignalRTrigger(HubName, "messages", "LeaveUserFromGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return UserGroups.RemoveFromGroupAsync(userName, groupName);
        }

        [Function("OnDisconnected")]
        public void OnDisconnected([SignalRTrigger(HubName, "connections", "disconnected")] SignalRInvocationContext invocationContext)
        {
            _logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
        }
    }

    public interface IChatClient
    {
        Task newMessage(NewMessage message);
        Task newConnection(NewConnection connection);
    }
}