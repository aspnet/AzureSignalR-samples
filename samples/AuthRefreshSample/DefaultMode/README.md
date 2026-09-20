# Authentication Refresh in Default Mode

This ASP.NET Core app hosts a SignalR hub through Azure SignalR Service in Default mode. It enables authentication refresh so the shared `Client/` can update an existing connection's authentication without reconnecting.

## Prerequisites

- .NET 11 preview SDK.
- An Azure SignalR Service resource in Default mode.
- Preview builds of the SignalR client and `Microsoft.Azure.SignalR`.

## Configure

Set the Azure SignalR connection string with user secrets:

```bash
dotnet user-secrets set "Azure:SignalR:ConnectionString" "<your-asrs-connection-string>"
```

## Run

Start this server:

```bash
dotnet run
```

Then run the shared client:

```bash
cd ../Client
dotnet run -- http://localhost:5000/chat alice user
```

Leave the client connected. Approximately every 90 seconds, it obtains a new application token and posts it to `/chat/refresh`; the Azure SignalR SDK updates the existing connection and returns a new service access token. The connection ID does not change.

Type `/refresh` in the client to refresh authentication manually.
