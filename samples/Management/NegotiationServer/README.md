# Negotiation Server

This sample shows how to use [Microsoft.Azure.SignalR.Management](https://www.nuget.org/packages/Microsoft.Azure.SignalR.Management) to host negotiation endpoint for SignalR clients.

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
dotnet add package Microsoft.Azure.SignalR.Management -v 1.0.0-*
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

### Create instance of `IServiceManger`

`IServiceManager` provides methods to generate client endpoints and access tokens for SignalR clients to connect to Azure SignalR Service. Add this constructor to the `NegotiateController` class.

```
private readonly IServiceManager _serviceManager;

public NegotiateController(IConfiguration configuration)
{
    var connectionString = configuration["Azure:SignalR:ConnectionString"];
    _serviceManager = new ServiceManagerBuilder()
        .WithOptions(o => o.ConnectionString = connectionString)
        .Build();
}
```

### Provide Negotiation Endpoint

In the `NegotiateController` class, provide the negotiation endpoint `/<Hub Name>/negotiate?user=<User ID>`.  

We use the `_serviceManager` to generate a client endpoint and an access token and return to SignalR client following [Negotiation Protocol](https://github.com/aspnet/SignalR/blob/master/specs/TransportProtocols.md#post-endpoint-basenegotiate-request), which will redirect the SignalR client to the service. 

>  You only need to provide a negotiation endpoint, since SignalR clients will reach the `/<Hub Name>/negotiate` endpoint for redirecting, if you provide a hub endpoint `/<Hub Name>` to SignalR clients.

```C#
[HttpPost("{hub}/negotiate")]
public ActionResult Index(string hub, string user)
{
    if (string.IsNullOrEmpty(user))
    {
        return BadRequest("User ID is null or empty.");
    }

    return new JsonResult(new Dictionary<string, string>()
    {
        { "url", _serviceManager.GetClientEndpoint(hub) },
        { "accessToken", _serviceManager.GenerateClientAccessToken(hub, user) }
    });
}
```

## Full Sample

The full negotiation server sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management#start-the-negotiation-server>).