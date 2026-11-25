# Negotiation Server

This sample shows how to use [Microsoft.Azure.SignalR.Management](https://www.nuget.org/packages/Microsoft.Azure.SignalR.Management) to host negotiation and authentication refresh endpoints for SignalR clients. Authentication refresh requires version 1.34.0 or later.

> You can use [Azure Functions](<https://azure.microsoft.com/en-us/services/functions/>) or other similar product instead to provide a totally serverless environment.
>
> For details what is negotiation and why we need a negotiation endpoint can be found [here](<https://github.com/Azure/azure-signalr/blob/dev/docs/management-sdk-guide.md#quick-start>).

## Build from Scratch

### create a webapi app

```
dotnet new webapi
```

### Add Management SDK to your project

```
dotnet add package Microsoft.Azure.SignalR.Management -v "1.*"
```

### Create a controller for negotiation

```C#
namespace NegotiationServer.Controllers
{
    [ApiController]
    public class NegotiateController : ControllerBase
    {
        ...
    }
}
```

### Create instance of `ServiceHubContext`

`ServiceHubContext` provides methods to generate client endpoints and access tokens for SignalR clients to connect to Azure SignalR Service. Wrap `ServiceHubContext` into a [`IHostedService`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-5.0) called `SignalRService` so that `ServiceHubContext` can be started and disposed when the web host starts and stops.

In `SignalRService` class, create the `ServiceHubContext`. In the sample we have two hub, message hub and chat hub to demostrate how to set up multiple hubs. The chat hub is actually not used.

```C#
public async Task StartAsync(CancellationToken cancellationToken)
{
    using var serviceManager = new ServiceManagerBuilder()
        .WithConfiguration(_configuration)
        .WithLoggerFactory(_loggerFactory)
        .BuildServiceManager();
    MessageHubContext = await serviceManager.CreateHubContextAsync(MessageHub, cancellationToken);
    ChatHubContext = await serviceManager.CreateHubContextAsync(ChatHub, cancellationToken);
}
```

Don't forget to dispose it when the hosted service stopped.


```C#
public Task StopAsync(CancellationToken cancellationToken) => HubContext?.DisposeAsync() ?? Task.CompletedTask;
```

### Provide Negotiation Endpoint

In the `NegotiateController` class, provide the negotiation endpoint `/negotiate?user=<User ID>`.

We use the `_hubContext` to generate a client endpoint and an access token and return to SignalR client following [Negotiation Protocol](https://github.com/aspnet/SignalR/blob/master/specs/TransportProtocols.md#post-endpoint-basenegotiate-request), which will redirect the SignalR client to the service.
```C#
[HttpPost("negotiate")]
public async Task<ActionResult> Index(string user)
{
    if (string.IsNullOrEmpty(user))
    {
        return BadRequest("User ID is null or empty.");
    }

    var negotiateResponse = await _hubContext.NegotiateAsync(new() { UserId = user });

    return new JsonResult(new Dictionary<string, string>()
    {
        { "url", negotiateResponse.Url },
        { "accessToken", negotiateResponse.AccessToken }
    });
}
```

The sample above uses the default negotiation options. If you want to return detailed error messages to clients, you can set `EnableDetailedErrors` as follows:

```C#
var negotiateResponse = await serviceHubContext.NegotiateAsync(new()
{
    UserId = user,
    EnableDetailedErrors = true
});
```
> `EnableDetailedErrors` defaults to false because these exception messages can contain sensitive information.

### Refresh an existing connection

Client-side authentication refresh requires SignalR client SDK 11.0.0 or later.

The sample uses `NegotiateWithTokenLifetimeAsync` to return `tokenLifetimeSeconds`,
which the client SDK uses to schedule automatic authentication refresh.

Refresh-aware clients can post to `/message/refresh` or `/chat/refresh`, passing the
connection token (not the public connection ID) in the `X-SignalR-Connection-Token`
header. The response contains `accessToken` and `tokenLifetimeSeconds`.

For example, a .NET SignalR client 11.0.0 or later can enable automatic refresh when building
the connection:

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/message?user=alice")
    .WithAuthenticationRefresh(options => options.EnableAutoRefresh = true)
    .Build();

await connection.StartAsync();
await connection.RefreshAuthenticationAsync(); // Optional immediate refresh.
```

The SDK sends the connection-token header automatically. Refresh renews the service
token while preserving the existing user and claims, without reconnecting. This
sample does not authenticate application users; the query-string user ID is for
demonstration only. In production, authenticate and authorize both negotiate and
refresh requests, and derive user identity and authentication expiration from the
validated application identity.

Missing connection tokens return HTTP 400. SDK refresh failures are logged and
return HTTP 500 without exposing exception details. No cross-origin browser access
is enabled; a browser client on another origin requires an explicit trusted-origin
CORS policy allowing `POST` and `X-SignalR-Connection-Token`.

## Full Sample

The full negotiation server sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management#start-the-negotiation-server>).