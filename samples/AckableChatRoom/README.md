# Ackable chat room sample for Azure SignalR service

This sample shows how to ack messages when using SignalR.

## Prerequisites
* Install .NET Core SDK
* Provision an Azure SignalR Service instance

Set the connection string in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#secret-manager) tool for .NET Core, and run this app.

```
dotnet restore
dotnet user-secrets set Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

Visit http://localhost:5000 to see the messages are delivered.