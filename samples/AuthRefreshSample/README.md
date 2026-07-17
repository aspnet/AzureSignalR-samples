# Auth Refresh Sample (Default mode)

A minimal ASP.NET Core **server** + .NET console **client** that demonstrate Azure SignalR
**Authentication Refresh** — refreshing an expiring SignalR auth token on a schedule **without
reconnecting** the client.

- The client connects with a short-lived **app token** (a demo JWT it mints itself).
- The server opts the hub into refresh (`EnableAuthenticationRefresh`) and tears the connection down
  when auth expires (`CloseOnAuthenticationExpiration`).
- Before the token expires, the client's `WithAuthenticationRefresh` auto-scheduler re-mints a fresh
  app token and POSTs `{hubUrl}/refresh`; Azure SignalR extends the live connection's deadline and the
  client adopts the refreshed **service** access token — the connection stays open the whole time.

> [!IMPORTANT]
> Authentication Refresh is a **preview** feature. It requires the **.NET 11 preview SDK** and preview
> builds of ASP.NET Core SignalR and `Microsoft.Azure.SignalR`.

## Layout

| Path | What |
| --- | --- |
| `Server/` | ASP.NET Core app server: JWT auth, `AddAzureSignalR()`, `ChatHub` with refresh enabled. |
| `Client/` | .NET console client using `WithAuthenticationRefresh`. |

Both share `DemoAuth.cs` (issuer/audience/HS256 key) so the client can mint tokens the server validates.
This is **demo-only**; a real app gets its app token from an identity provider.

## Prerequisites

- .NET 11 preview SDK.
- An Azure SignalR Service resource (connection string).

## Configure the connection string (server)

Set `Azure:SignalR:ConnectionString` — for local dev, user secrets or an environment variable:

```bash
cd Server
dotnet user-secrets init
dotnet user-secrets set "Azure:SignalR:ConnectionString" "<your-asrs-connection-string>"
# or:  setx Azure__SignalR__ConnectionString "<your-asrs-connection-string>"
```

## Run

In one terminal:

```bash
cd Server
dotnet run
```

In another:

```bash
cd Client
dotnet run
# optional args:  dotnet run -- http://localhost:5000/chat alice user
```

Type messages in the client to broadcast them. Roughly every ~90s (2 min token, refresh 30s before
expiry) you'll see:

```
[refresh] succeeded; next lifetime = 00:02:00
system: auth refreshed for alice
```

...while the connection never drops.

## Try the accept/reject gate

The server rejects a refresh whose new token carries role `blocked`:

```csharp
options.OnAuthenticationRefresh = context =>
    ValueTask.FromResult(!context.NewUser.IsInRole("blocked"));
```

Start the client with the `blocked` role to see the refresh fail with `403 permission_change_rejected`
(the existing connection is left open, unchanged, until its deadline):

```bash
cd Client
dotnet run -- http://localhost:5000/chat alice blocked
```

```
[refresh] FAILED: ...permission_change_rejected...
```

## How it works

1. **Negotiate.** The client sends its app token to `/chat/negotiate`; the server validates it, and
   because refresh is enabled for an authenticated principal with an expiry, advertises
   `tokenLifetimeSeconds`.
2. **Connect.** The client connects to Azure SignalR with the returned service access token.
3. **Schedule.** `WithAuthenticationRefresh` schedules a refresh before `tokenLifetimeSeconds` elapses.
4. **Refresh.** The client re-mints a fresh app token (`AccessTokenProvider`) and POSTs
   `/chat/refresh?id={connectionToken}`. The server runs the optional `OnAuthenticationRefresh` gate,
   then asks Azure SignalR to extend the live connection's auth deadline and apply the refreshed claims.
5. **Adopt.** The server returns `{ accessToken, tokenLifetimeSeconds }`; the client adopts the new
   service token and schedules the next refresh. The connection is never reconnected.

> [!NOTE]
> The demo surfaces the app token's `exp` as the auth ticket's `ExpiresUtc` in `OnTokenValidated`(JwtBearer doesn't do this by default), which is what lets negotiate advertise `tokenLifetimeSeconds`.
