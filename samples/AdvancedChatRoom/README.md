Azure SignalR Service Advanced Chat Room
=================================

Just like [ChatRoom sample](../ChatRoom), you can leverage Azure SignalR Service to handle more clients and offload the connection management part. This sample demonstrates more operations available in Azure SignalR Service.

Now the sample supports:

* Echo
* Broadcast
* Join Group / Leave Group
* Send to Group / Groups / Group except connection
* Send to User / Users
* Cookie / JWT based Authentication
* Role / Claim / Policy based Authrization
* JWT authentication refresh without reconnecting

## Run the sample locally

Authentication refresh requires SignalR JavaScript client 11.0.0 or later and Azure SignalR SDK 1.34.0 or later.

Type the following commands to run this app.
```powershell
dotnet restore
$env:Azure__SignalR__ConnectionString = "<your connection string>"
dotnet run
```

## Run via Docker
Build the image from this sample directory:

```
docker build -t advancedchatroom .
docker run -e Azure__SignalR__ConnectionString="<signalr-connection-string>" \
           -p 5000:80 advancedchatroom
```

Open the broswer with url `localhost:5000`, you can see the sample just like Chat Sample but has more operations. 
