## Build new testing image

```
./build-image.sh <sdk-version>
```

## Test with image
- Run command
```
docker run -ti -e ConnectionString=<connection-string> -p 5000:5000 signalr-sdk-fullsample:<sdk-version>
```
- Open broswer with url `localhost:5000`