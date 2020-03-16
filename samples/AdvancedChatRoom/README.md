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

## Run the sample locally

Type the following commands to run this app.
```
dotnet restore
dotnet user-secrets set Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

Open the broswer with url `localhost:5000`, you can see the sample just like Chat Sample but has more operations. 
