Azure SignalR Service Serverless Sample
=================================

This sample demostrate a serverless Azure SignalR Service. One console app call REST API in service to send message and another console app work as a client to receive message through listening the service.

The sample also demostrate how to generate access token to communicate with service.

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

After the server started, use the command to send message

```
send user <User Id>
send users <User List>
send group <Group Name>
send groups <Group List>
broadcast
```
