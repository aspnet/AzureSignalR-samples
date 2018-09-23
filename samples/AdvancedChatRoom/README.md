Azure SignalR Service Advanced Chat Room
=================================

Just like [ChatRoom sample](../ChatRoom), you can leverage Azure SignalR Service to handle more clients and offload the connection management part. This sample demonstrates more operations available in Azure SignalR Service. Also provided docker image to simplify build environment problems.

Now the sample supports:

* Echo
* Broadcast
* Join Group / Leave Group
* Send to Group / Groups / Group except connection
* Send to User / Users
* Cookie / JWT based Authentication
* Role / Claim / Policy based Authrization

## Build Docker Images

First check the SDK version needed. Each SDKVerison will build a separate docker image and the tag is the sdk version.
```bash
# build-image.sh
declare -a SDKVersion=("1.0.0-preview1-10009" "1.0.0-preview1-10011" "1.0.0-preview1-10015" "1.0.0-preview1-10200")
```

Then run the script to build docker images.
```bash
./build-image.sh
```

You can run the image with Azure Signalr Service.
```bash
docker run -e Azure__SignalR__ConnectionString="<your connection string>" -p 5000:80 signalr-advancedchatroom:<sdk version>
```

Open the broswer with url `localhost:5000`, you can see the sample just like Chat Sample but has more operations. 
