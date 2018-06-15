Azure SignalR Service Full Sample
=================================

Just like [ChatRoom sample](../ChatRoom), you can leverage Azure SignalR Service to handle more clients and offload the conncetion management part. This sample demonstrates more operations available in Azure SignalR Service. Also provided docker image to simplify build environment problems.

## Build Docker Images

First check the SDK version needed. Each SDKVerison will build a separate docker image.
```bash
# build-image.sh
declare -a SDKVersion=("1.0.0-preview1-10009" "1.0.0-preview1-10011")
```

Then run the script to build docker images.
```bash
./build-image.sh
```

You can run the test the image with the signalr service.
```bash
docker run -ti -e Azure__SignalR__ConnectionString=<SDKVersion> -p 5000:80 signalr-sdk-fullsample:<sdk-version>
```

Open the broswer with url `localhost:5000`, you can see the sample just like Chat Sample but has more operations. 
