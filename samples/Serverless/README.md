Azure SignalR Service Serverless Sample
=================================

## **Deprecated**

This sample have been **deprecated**. Users are not recommended to generate access token by themselves. In the Serverless scenario, users are recommended to use Management SDK to build a negotiation server which is responsible for generating access token. For more samples and details, take [Management Sample](../Management) as reference.

----

This sample is a console app showing the use of Azure SignalR Service in server-less pattern. It provides two modes:

- Server Mode: use simple commands to call Azure SignalR Service REST API.
- Client Mode: connect to Azure SignalR Service and receive messages from server.

Also you can find how to generate an access token to authenticate with Azure SignalR Service.

## Run the sample

### Fist to build the executive file.

```
dotnet publish -c Release -r win10-x64
```

### Start a client

```
Serverless.exe client <ClientName> -c "<ConnectionString>" -h <HubName>
```

### Start a server

```
Serverless.exe server -c "<ConnectionString>" -h <HubName>
```

## Run the sample without publishing

You can just run the command below to start a server or client

```
# Start a server
dotnet run -- server -c "<ConnectionString>" -h <HubName>

# Start a client
dotnet run -- client <ClientName> -c "<ConnectionString>" -h <HubName>
```

### Use user-secrets to specify Connection String

You can run `dotnet user-secrets set Azure:SignalR:ConnectionString "<ConnectionString>"` in the root directory of the sample. After that, you don't need the option `-c "<ConnectionString>"` anymore.

## Usage

After the server started, use the command to send message

```
send user <User Id>
send users <User List>
send group <Group Name>
send groups <Group List>
broadcast
```
