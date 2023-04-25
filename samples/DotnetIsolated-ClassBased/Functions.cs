using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;

namespace DotnetIsolated_ClassBased
{
    public class Functions : ServerlessHub<IChatClient>
    {
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
        public Task OnConnected([SignalRTrigger(nameof(Functions), "connections", "connected")] SignalRInvocationContext invocationContext)
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            _logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            return Clients.All.newConnection(new NewConnection(invocationContext.ConnectionId, auth));
        }

        [Function("Broadcast")]
        public Task Broadcast(
        [SignalRTrigger(nameof(Functions), "messages", "broadcast", "message")] SignalRInvocationContext invocationContext, string message)
        {
            return Clients.All.newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToGroup")]
        public Task SendToGroup([SignalRTrigger(nameof(Functions), "messages", "SendToGroup", "groupName", "message")] SignalRInvocationContext invocationContext, string groupName, string message)
        {
            return Clients.Group(groupName).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToUser")]
        public Task SendToUser([SignalRTrigger(nameof(Functions), "messages", "SendToUser", "userName", "message")] SignalRInvocationContext invocationContext, string userName, string message)
        {
            return Clients.User(userName).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("SendToConnection")]
        public Task SendToConnection([SignalRTrigger(nameof(Functions), "messages", "SendToConnection", "connectionId", "message")] SignalRInvocationContext invocationContext, string connectionId, string message)
        {
            return Clients.Client(connectionId).newMessage(new NewMessage(invocationContext, message));
        }

        [Function("JoinGroup")]
        [SignalROutput(HubName = nameof(Functions))]
        public Task JoinGroup([SignalRTrigger(nameof(Functions), "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return Groups.AddToGroupAsync(connectionId, groupName);
        }

        [Function("LeaveGroup")]
        [SignalROutput(HubName = nameof(Functions))]
        public Task LeaveGroup([SignalRTrigger(nameof(Functions), "messages", "LeaveGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return Groups.RemoveFromGroupAsync(connectionId, groupName);
        }

        [Function("JoinUserToGroup")]
        [SignalROutput(HubName = nameof(Functions))]
        public Task JoinUserToGroup([SignalRTrigger(nameof(Functions), "messages", "JoinUserToGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return UserGroups.AddToGroupAsync(userName, groupName);
        }

        [Function("LeaveUserFromGroup")]
        [SignalROutput(HubName = nameof(Functions))]
        public Task LeaveUserFromGroup([SignalRTrigger(nameof(Functions), "messages", "LeaveUserFromGroup", "userName", "groupName")] SignalRInvocationContext invocationContext, string userName, string groupName)
        {
            return UserGroups.RemoveFromGroupAsync(userName, groupName);
        }

        [Function("OnDisconnected")]
        public void OnDisconnected([SignalRTrigger(nameof(Functions), "connections", "disconnected")] SignalRInvocationContext invocationContext)
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