Azure SignalR Service Management SDK Sample 
=================================

This sample shows the use of Azure SignalR Service Management SDK.

* Message Pulbisher: shows how to publish messages to SignalR clients using Management SDK.
* Negotitation Server: shows how to negotiate client from you app server to Azure SignalR Service using Management SDK.
* SignalR Client: is a tool to start multiple SignalR clients and these clients listen messages for this sample.

## Run the sample

### Start the negotiation server

```
cd NegotitationServer
setx Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

## Start SignalR clients

```
cd SignalRClient
dotnet run
```

Parameters:
* -h|--hubEndpoint: Set hub endpoint. Default value: "http://localhost:5000/Management".
* -u|--user: Set user ID. Default value: "User". You can set multiple users like this: "-u user1 -u user2".

## Start Message Publisher

```
cd MessagePublisher
dotnet run
```

Parameters:
* -c|--connectionstring: Set connection string.
* -t|--transport: Set service transport type. Options: \<transient\>|\<persistent\>. Default value: transient. `Transient`: calls REST API for each message. `Persistent`: Establish a WebSockets connection and send all messages in the connection.

After the publisher started, use the command to send message

```
send user <User ID List (Seperated by ',')> <Message>
send users <User List> <Message>
send group <Group Name> <Message>
send groups <Group List (Seperated by ',')> <Message>
usergroup add <User ID> <Group Name>
usergroup remove <User ID> <Group Name>
broadcast <Message>
```

### Use user-secrets to specify Connection String

You can run `dotnet user-secrets set Azure:SignalR:ConnectionString "<Connection String>"` in the root directory of the sample. After that, you don't need the option `-c "<Connection String>"` anymore.
