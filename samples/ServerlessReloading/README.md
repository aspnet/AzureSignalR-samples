Azure SignalR Service Connection Reloading Sample(Service Graceful Shutdown).
=================================

This sample shows the use of Azure SignalR Service Management SDK to simulate service graceful shutdown and client connection reloading under serverless mode.

* Message Publisher: shows how to publish messages to SignalR clients or to simulate service's graceful shutdown with REST api using Management SDK.
* Negotiation Server: shows how to negotiate client from you app server to Azure SignalR Service using Management SDK.
* SignalR Client: is a tool to start multiple SignalR clients(supporting reloading feature) and these clients listen messages for this sample.

## Run the sample

### Start the negotiation server

```
cd NegotitationServer
dotnet user-secrets set Azure:SignalR:ConnectionString "<Connection String>"
dotnet run
```

### Start SignalR clients

```
cd SignalRClient
dotnet run
```

>  Parameters:
> 
> - -h|--hubEndpoint: Set hub endpoint. Default value: "<http://localhost:5000/ManagementSampleHub>".
> - -u|--user: Set user ID. Default value: "User". You can set multiple users like this: "-u user1 -u user2".

### Start message publisher

```
cd MessagePublisher
dotnet run

```

> Parameters:
> 
> -c|--connectionstring: Set connection string.
> -t|--transport: Set service transport type. Options: <transient>|<persistent>. Default value: transient. Transient: calls REST API for each message. Persistent: Establish a WebSockets connection and send all messages in the connection.

Once the message publisher get started, use the command to send message. If you want to switch all connections from one service instance to another, just enter 'reload'

```
send user <User ID List (Seperated by ',')> <Message>
send users <User List> <Message>
send group <Group Name> <Message>
send groups <Group List (Seperated by ',')> <Message>
usergroup add <User ID> <Group Name>
usergroup remove <User ID> <Group Name>
broadcast <Message>
reload
```
 For example, type `broadcast hello`, and press keyboard `enter` to publish messages.

You will see `User: gets message from service: 'hello'` from your SignalR client tool.

### Use `user-secrets` to specify Connection String

You can run `dotnet user-secrets set Azure:SignalR:ConnectionString "<Connection String>"` in the root directory of the sample. After that, you don't need the option `-c "<Connection String>"` anymore.

## Build Management Sample from Scratch

The following links are guides for building 3 components of this management sample from scratch.

* [Message Publisher](./MessagePublisher/README.md)
* [Negotiation Server](./NegotiationServer/README.md)
* [SignalR Client](./SignalRClient/README.md)