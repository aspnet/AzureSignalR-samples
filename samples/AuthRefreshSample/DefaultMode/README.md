# Authentication Refresh in Default Mode

This ASP.NET Core app hosts a SignalR hub through Azure SignalR Service in Default mode. It enables authentication refresh so the shared `Client/` can update an existing connection's authentication without reconnecting.

## Prerequisites

- .NET 11 GA SDK (11.0.100 or later)
- An Azure SignalR Service resource
- `Microsoft.AspNetCore.SignalR.Client` 11.0.0
- Azure SignalR SDK packages 1.34.0

## Configure

Set the connection string in the terminal where you will run the server:

```powershell
$env:Azure__SignalR__ConnectionString = "<your-connection-string>"
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
