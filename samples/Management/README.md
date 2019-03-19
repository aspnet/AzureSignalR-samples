Azure SignalR Service Management SDK Sample 
=================================

This sample is a console app showing the use of Azure SignalR Service Management SDK. It provides two modes:

- Server Mode: use simple commands to send messages to SignalR clients and manage group membership for users.
- Client Mode: connect to Azure SignalR Service and receive messages from server.

Also you can find how to generate an access token to authenticate with Azure SignalR Service.

## Run the sample

### Fist to build the executive file.

```
dotnet publish -c Release -r win10-x64
```

### Start a client

```
Management.exe client <ClientName> -c "<ConnectionString>" -h <HubName> 
```

### Start a server

```
Management.exe server -c "<ConnectionString>" -h <HubName> -t <Service Transport Type: transient/persistent>
```

## Run the sample without publishing

You can just run the command below to start a server or client

```
# Start a server
dotnet run -- server -c "<ConnectionString>" -h <HubName> -t <Service Transport Type: transient/persistent>

# Start a client
dotnet run -- client <ClientName> -c "<ConnectionString>" -h <HubName>
```

### Use user-secrets to specify Connection String

You can run `dotnet user-secrets set Azure:SignalR:ConnectionString "<ConnectionString>"` in the root directory of the sample. After that, you don't need the option `-c "<ConnectionString>"` anymore.

## Usage

After the server started, use the command to send message

```
send user <User Id List (Seperate with ',')>
send users <User List>
send group <Group Name>
send groups <Group List (Seperate with ',')>
usergroup add <User Id> <Group Name>
usergroup remove <User Id> <Group Name>
broadcast
```
