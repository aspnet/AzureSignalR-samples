Message Publisher
=========

This sample shows how to use [Microsoft.Azure.SignalR.Management](https://www.nuget.org/packages/Microsoft.Azure.SignalR.Management) to publish messages to SignalR clients that connect to Azure SignalR Service.

## Build from Scratch

### Add Management SDK to your project

```
dotnet add package Microsoft.Azure.SignalR.Management -v 1.*
```

### Create instance of `ServiceManager`

The `ServiceManager` is able to manage your Azure SignalR Service.

```c#
var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
{
    option.ConnectionString = _connectionString;
    option.ServiceTransportType = _serviceTransportType;
})
//Uncomment the following line to get more logs
//.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
.BuildServiceManager();
```

### Create instance of `ServiceHubContext`

The `ServiceHubContext` is used to publish messages to a specific hub.

```C#
var hubContext = await serviceManager.CreateHubContextAsync("<Your Hub Name>");
```

### Publish messages to a specific hub

Once you create the `hubContext`, you can use it to publish messages to a given hub.

```C#
// broadcast
hubContext.Clients.All.SendAsync("<Your SignalR Client Callback>", "<Arg1>", "<Arg2>", ...);

// send to a user
hubContext.Clients.User("<User ID>").SendAsync("<Your SignalR Client Callback>", "<Arg1>", "<Arg2>", ...);

// send to users
hubContext.Clients.Users(<User ID List>).SendAsync("<Your SignalR Client Callback>", "<Arg1>", "<Arg2>", ...);

// send to a group
hubContext.Clients.Group("<Group Name>").SendAsync("<Your SignalR Client Callback>", "<Arg1>", "<Arg2>", ...);

// send to groups
hubContext.Clients.Group(<Group Name List>).SendAsync("<Your SignalR Client Callback>", "<Arg1>", "<Arg2>", ...);

// add a user to a group
hubContext.UserGroups.AddToGroupAsync("<User ID>", "<Group Name>");

// remove a user from a group
hubContext.UserGroups.RemoveFromGroupAsync("<User ID>", "<Group Name>");

...
```

All features can be found [here](<https://github.com/Azure/azure-signalr/blob/dev/docs/management-sdk-guide.md#features>).

### Dispose the instance of `ServiceHubContext`

```c#
await hubContext.DisposeAsync();
```

## Full Sample

The full message publisher sample can be found [here](.). The usage of this sample can be found [here](<https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management#start-message-publisher>).