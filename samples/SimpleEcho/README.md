# Azure SignalR Service Simple Echo

This is the simplest sample that shows how to send a message back and forth between the client and the server through Azure SignalR Service. This sample can be also used as the connectivity check tool.

## Prerequisites
* Install .NET Core SDK
* Provision an Azure SignalR Service instance

Set the connection string in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#secret-manager) tool for .NET Core, and run this app.

```
dotnet restore
dotnet user-secrets set Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

After running, you will see that the web server starts, makes connections to the Azure SignalR Service instance and creates an endpoint at `http://localhost:5000/echo`. Then a client will be launched and sends a hello message to it. If everything goes well you will see `Hello!` printed on the screen.