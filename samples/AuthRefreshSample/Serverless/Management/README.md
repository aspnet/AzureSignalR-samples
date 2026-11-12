# Authentication Refresh in Serverless Mode with the Management SDK

This ASP.NET Core app is the serverless authentication boundary for Azure SignalR Service. It uses the Management SDK to negotiate a connection and refresh its authentication without reconnecting. The shared `Client/` is used unchanged.

## Prerequisites

- .NET 11 GA SDK (11.0.100 or later)
- An Azure SignalR Service resource
- `Microsoft.AspNetCore.SignalR.Client` 11.0.0
- Azure SignalR SDK packages 1.34.0

## Configure

Set the Azure SignalR connection string with user secrets:

```bash
dotnet user-secrets set "Azure:SignalR:ConnectionString" "<your-asrs-connection-string>"
```

`ServiceTransportType` defaults to `Transient`. Change it to `Persistent` in `appsettings.json` to exercise the persistent Management SDK transport; the application code is the same for both.

## Run

Start this server:

```bash
dotnet run
```

Then run the same client used by the Default-mode sample:

```bash
cd ../../Client
dotnet run -- http://localhost:5000/chat alice user
```

Leave the client connected. Approximately every 90 seconds, it obtains a new application token and posts it to `/chat/refresh`; the Management SDK updates the existing connection and returns a new service access token. The connection ID does not change.

The sample advertises the refresh lifetime through `NegotiateWithTokenLifetimeAsync`, configures a one-hour maximum service-token lifetime, and passes the application ticket's absolute expiration separately through `NegotiationOptions.AuthenticationExpiresOn`. Refresh uses `RefreshConnectionAuthenticationOptions` to provide the new expiration, projected user and role claims, and the same one-hour service-token maximum. Because the demo application token expires in two minutes, the Management SDK mints the service token with the shorter remaining application-authentication lifetime.

The refresh endpoint maps an unknown connection to `404 connection_not_found`, a blocked or
different user to `403 permission_change_rejected`, invalid expiration to `400 invalid_expiration`,
and unexpected service failures to `500 internal_server_error`.

Type `/refresh` in the client to refresh authentication manually.

## Endpoints

| Route | Purpose |
| --- | --- |
| `POST /chat/negotiate` | Validates the application token and calls `NegotiateWithTokenLifetimeAsync`. |
| `POST /chat/refresh` | Requires the connection token in the `X-SignalR-Connection-Token` header, validates the new application token, and calls `RefreshConnectionAuthenticationAsync`. |

The custom header forces cross-origin browser requests to complete a CORS preflight. This sample uses a .NET client and does not enable cross-origin browser access. If adding a browser client, configure CORS for the trusted client origins and allow `POST`, `Authorization`, and `X-SignalR-Connection-Token`.
