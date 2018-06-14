## Build new testing image

```
./build-image.sh
```

## Test with image
- Run command
```
docker run -ti -e Azure__SignalR__ConnectionString=<SDKVersion> -p 5000:5000 signalr-sdk-fullsample:<sdk-version>
```
- Open broswer with url `localhost:5000`
