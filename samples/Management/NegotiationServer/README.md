# Negotiation Server

This sample shows how to use [Microsoft.Azure.SignalR.Management](https://www.nuget.org/packages/Microsoft.Azure.SignalR.Management) (version>=1.10.0) to host negotiation endpoint for SignalR clients.

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
dotnet add package Microsoft.Azure.SignalR.Management -v 1.*
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
## Full Sample

The full negotiation server sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management#start-the-negotiation-server>).