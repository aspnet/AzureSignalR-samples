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
dotnet run -- -c "<Connection String>"
```

## Start SignalR clients

```
cd SignalRClient
dotnet run -- -n "<Neogotiation Endpoint>" -u <User ID A> -u <User ID B>
```

## Start Message Publisher

```
cd MessagePublisher
dotnet run -- -c "<Connection String>" -t <Service Transport Type>

```

> \<Service Transport Type\>: `transient` or `persistent`. `transient` is the default value.

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
