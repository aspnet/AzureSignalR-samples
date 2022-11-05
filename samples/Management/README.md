Azure SignalR Service Management SDK Sample
=================================

This sample shows the usage of Azure SignalR Service Management SDK.

* Message Publisher: shows how to publish messages to SignalR clients using Management SDK.
* Negotiation Server: shows how to negotiate client from you app server to Azure SignalR Service using Management SDK.
* SignalR Client: is a tool to start multiple SignalR clients and these clients listen messages for this sample.

## Run the sample

### Start the negotiation server

```
cd NegotiationServer
dotnet user-secrets set Azure:SignalR:ConnectionString "<Connection String>"
dotnet run
```

>  Parameters of `dotnet run`
>
> --enableDetailedErrors: true to enable log detailed errors on client side, false to disable. The default value is false, as detailed errors might contain sensitive information. This is useful if you want client connection to get the exception on close.

### Start SignalR clients

```
cd SignalRClient
dotnet run
```

>  Parameters:
>
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

Once the message publisher get started, use the command to send message

```
send user <UserId> <Message>
send users <User1,User2,...> <Message>
send group <GroupName> <Message>
send groups <Group1,Group2,...> <Message>
usergroup add <User1,User2,...> <GroupName>
usergroup remove <UserId> <GroupName>
broadcast <Message>
close <ConnectionID> <Reason>?
checkexist connection <ConnectionID>
checkexist user <UserID>
checkexist group <GroupName>
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
