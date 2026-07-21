# Azure SignalR Authentication Refresh Sample

This sample shows how a .NET SignalR client can refresh authentication for an existing Azure SignalR connection without reconnecting.

The client uses a short-lived application token. Before it expires `WithAuthenticationRefresh` obtains a new application token and posts it to `{hubUrl}/refresh`. The server updates the connection's authentication expiration and returns a new Azure SignalR service access token. The connection remains active throughout the refresh.

## Modes

### Default mode

`DefaultMode/` hosts a SignalR hub with `Microsoft.Azure.SignalR`. The server enables `EnableAuthenticationRefresh` and `CloseOnAuthenticationExpiration`; the Azure SignalR SDK handles the negotiate and refresh endpoints.

### Serverless mode

`Serverless/Management/` implements the negotiate and refresh endpoints directly. It uses `ServiceHubContext.NegotiateWithTokenLifetimeAsync` to negotiate and `ServiceHubContext.RefreshConnectionAuthenticationAsync` to refresh the live connection.

Both modes expose the same client-facing contract, so they reuse the client in `Client/`.

## Prerequisites

- .NET 11 preview SDK
- An Azure SignalR Service resource
- Preview SignalR and Azure SignalR SDK packages

Until a preview SignalR client package containing authentication refresh is published, clone `aspnetcore` beside this sample repository. The client project automatically references `aspnetcore/src/SignalR`; for another location, pass `-p:AspNetCoreSignalRSourceRoot=<path>`.

Use an Azure SignalR resource in Default mode with `DefaultMode/`, or in Serverless mode with `Serverless/Management/`.

## Configure

Set the connection string in the terminal where you will run the server:

```powershell
$env:Azure__SignalR__ConnectionString = "<your-connection-string>"
```

## Run

Start one server from the `AuthRefreshSample` directory.

Default mode:

```bash
dotnet run --project DefaultMode
```

Serverless mode with the Management SDK:

```bash
dotnet run --project Serverless/Management
```

Then start the shared client in another terminal:

```bash
dotnet run --project Client -- http://localhost:5000/chat alice user
```

Leave the client connected. It refreshes authentication approximately 30 seconds before each two-minute application token expires, without changing the connection ID.

> [!NOTE]
> The client's interactive `Broadcast` command requires the hosted hub in Default mode. In Serverless mode, client-to-server messages require an Azure SignalR upstream. Authentication refresh itself uses the same client in both modes.
