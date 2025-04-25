# SignalR Client

This sample shows how to use SignalR clients to connect Azure SignalR Service without using a web server that host a SignalR hub. The SignalrR client support reloading connection with the use of various types of messages in TypeClass.cs.

## Build from Scratch

### Add Management SDK to your project

```
dotnet add package Microsoft.Azure.SignalR.Management -v 1.0.0-*
```

### Connect SignalR clients to a hub endpoint with user ID

Here because we need client to support reloading feature, so we actually build a more powerful StableConnection class on the basis of HubConnection class.

```C# 
var connections = (from userId in userIds
select new StableConnection(hubEndpoint, userId)).ToList();
```

### Handle connection closed event in StableConnection class

Sometimes SignalR clients may be disconnected by Azure SignalR Service, the `Closed` event handler will be useful to figure out the reason.

```C#
connection.Closed += async ex =>
{
	// handle exception here
    ...
};
```

### Handle SignalR client callback in StableConnection class

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

The full refined message publisher version(supporting reloading feature) sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/ServerlessReloading#start-signalr-clients>).