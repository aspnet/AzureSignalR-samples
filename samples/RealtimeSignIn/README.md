# Realtime Sign-in Example using Azure SignalR Service

This sample application shows how to build a realtime application using Azure SignalR Service and serverless architecture. When you open the homepage of the application, you will see how many people have visited this page (and their OS and browser distribution) and the page will automatically update when others open the same page.

A live demo can be found [here](https://signalr-signin-demo.azurewebsites.net/).

## How Does It Work

The application is built on top of Azure SignalR Service, Functions and Storage. There is no web server needed in this sample.

Here is a diagram that illustrates the structure of this appliaction:

![architecture](../../docs/images/signin.png)

1. When user opens the homepage, a HTTP call will be made to an API exposed by Azure Function HTTP trigger, which will record your information and save it to Azure table storage.
2. This API also returns a url and token for browser to connect to Azure SignalR Service.
3. Then the API calculate statistics information (number of visits, OS and browser distribution) and use Azure SignalR Service to broadcast to all clients so browser can do a realtime update without need to do a refresh.
4. The static content (homepage, scripts) are stored in Azure blob storage and exposed to user through Azure Function proxy.

## Deploy to Azure

### Deploy Static Files

1. Create storage account

   ```
   az storage account create \
      --location <location> \
      --name <account_name> \
      --resource-group <resource_group> \
      --sku Standard_LRS
   ```

2. Create container and table

   ```
   az storage table create --account-name <account_name> --name SignInInfo
   az storage container create --account-name <account_name> --name content --public-access blob
   ```
   > Please take note of the endpoints created in the previous commands, as you will need it later to add to the Function App's application settings.

3. Navigate to the ```/content``` folder and upload the static files

   ```
   az storage blob upload -c content --account-name <account_name> -n index.html -f index.html
   az storage blob upload -c content --account-name <account_name> -n scripts/qrcode.min.js -f scripts/qrcode.min.js
   az storage blob upload -c content --account-name <account_name> -n images/signalr-logo.png -f images/signalr-logo.png
   ```


### Deploy Azure Function

1. Create a SignalR Service instance:

   ```
   az group create --name <resource_group_name> --location WestUS
   az signalr create --resource-group <resource_group_name> --name <signalr_name> --sku Standard_S1
   ```

2. Create Azure function

   ```
   az storage account create --resource-group <resource_group_name> --name <storage_account_name> \
      --location CentralUS --sku Standard_LRS
   az functionapp create --resource-group <resource_group_name> --name <function_name> \
      --consumption-plan-location CentralUS --storage-account <storage_account_name>
   ```

   > Please provide a unique name for ```function_name```

3. Navigate to the ```/function``` folder and configure the deployment credentials

   ```
   az functionapp deployment source config-local-git --resource-group <resource_group_name> --name <function_name>
   az functionapp deployment user set --user-name <user_name> --password <password>
   ```

   > Please take note of the Git url returned as you will need it later on.

4. Build and deploy Azure function

   ```
   nuget restore
   msbuild /p:Configuration=Release

   cd bin/Release/net461
   git init
   git remote add origin <deploy_git_url>
   git add -A
   git commit -m "init commit"
   git push origin master
   ```

5. Update application settings

   ```
   az functionapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
      --setting TableConnectionString=<table_connection_string>
   az functionapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
      --setting BlobHost=<blob_host>
   az functionapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
      --setting AzureSignalRConnectionString=<signalr_connection_string>

   ```

> ```table_connection_string``` can be located in the Azure Portal from the Access Key section of the created storage account. ```blob_host``` is the blob service endpoint hostname (without https://), which you can find in the Blob Service Containers section. ```signalr_connection_string``` refers to the connection string you used before in the Chatroom sample.

## Run the application

Go to the url that was deployed as part of your Function App:

   ```
   https://<function_name>.azurewebsites.net/
   ``` 

It should show you an output similar to this:

![Screenshot of the output in a web browser](screenshot.png)

## Brief Explanation

What's happening behind the scene is Azure SignalR Service exposed a set of REST APIs to for you to send message to clients. For example, the API that broadcasts message to all clients is exposed through this endpoint:

```
POST https://<service_endpoint>/api/v1/hubs/<hub_name>
```

The body of the request is a JSON object with two properties:

1. `target`: The target method you want to call in clients.
2. `arguments`: an array of arguments you want to send to clients.

The service authenticates REST call using JWT token, when you're generating the JWT token, make sure the audience is same as the REST API url and use the access key in SignalR service connection string as the secret key. Then put it in authentication header:

```
Authorization: Bearer <jwt_token>
```

> Refer to `GenerateJwtBearer()` in [AzureSignalR.cs](function/AzureSignalR.cs) for a sample to generate JWT token in C#.

If the API call succeeds, it returns 202 (Accepted).

Clients also connect to SignalR service using JWT token, since there is no web server to generate the token, you'll need to do it in Azure function as well.

1. The JWT token is generated using the same algorithm described above, just the audience should be the url of the client hub.
2. Client hub is the following format:

   ```
   https://<service_endpoint>/client/?hub=<hubName>
   ```

   This is also the endpoint client should connect to using SignalR client SDK.
