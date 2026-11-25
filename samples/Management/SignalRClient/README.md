# SignalR Client

This sample shows how to use SignalR clients to connect Azure SignalR Service without using a web server that host a SignalR hub.

Requires the .NET 11 SDK. Authentication refresh requires SignalR .NET client SDK 11.0.0 or later.

## Build from scratch


### Connect SignalR clients to a hub endpoint with user ID

```C#
var url = $"{hubEndpoint.TrimEnd('/')}?user={<User ID>}";
var connection = new HubConnectionBuilder()
    .WithUrl(url)
    .WithAuthenticationRefresh(options => options.EnableAutoRefresh = true)
    .Build();
```

The client automatically refreshes authentication based on `tokenLifetimeSeconds`
returned by the negotiation server, and logs refresh success or failure for each user.
The paired negotiation server renews the service access token while preserving
the connection's existing identity. Press Enter to stop the clients as before.

### Handle connection closed event

Sometimes SignalR clients may be disconnected by Azure SignalR Service, the `Closed` event handler will be useful to figure out the reason.

```C#
connection.Closed += async ex =>
{
  // handle exception here
    ...
};
```

### Handle SignalR client callback

```C#
connection.On("<SignalR Client Callback>", (<Type> <arg1>, <Type> <arg2>, ...) =>
{
  // handle received arguments
  ...
});
```

### Establish connection to Azure SignalR Service

```C#
await connection.StartAsync();
```

Once your SignalR clients connect to the service, it is able to listen messages.

### Stop SignalR clients

You can stop the client connection anytime you want.

```C#
await connection.StopAsync();
```

## Full Sample

The full message publisher sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management#start-signalr-clients>).