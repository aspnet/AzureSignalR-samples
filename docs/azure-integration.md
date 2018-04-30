# Integrate with Azure Services

In the [ChatRoom sample](../ChatRoom) you have learned how to use Azure SignalR Service in your application, but you still need to host the hub implementation on a web server.
In this tutorial you'll learn how to use Azure web app to host your hub logic and also integrate with serverless applications like Azure functions.

## Deploy SignalR Hub to Azure Web App

Azure Web App is a service for hosting web applications, which is a perfect choice for hosting our SignalR hub.
Azure Web App supports container, so we will build our application into a Docker container and deploy it to web app.

### Build Docker Image

First use the [Dockerfile](../samples/ChatRoom/Dockerfile) to build our application into a Docker container image:

```
docker build -t chatroom .
```

Let's take a look at the details of the Dockerfile.

First copy the source code, restore, build and publish the app:

```docker
# copy csproj and restore as distinct layers
COPY NuGet.config ./
RUN mkdir ChatRoom && cd ChatRoom/
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY ./ ./
RUN dotnet publish -c Release -o out
```

Then copy the build output into `app` folder and set the entrypoint:

```docker
# build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ChatRoom.dll"]
```

Then you can test the image locally:

```
docker run -p 5000:5000 -e Azure__SignalR__ConnectionString=<connection_string> chatroom
```

> For more information about building Docker image for .NET Core, please refer to this [doc](https://docs.microsoft.com/en-us/dotnet/core/docker/building-net-docker-images).

After you test the image, push it to a Docker register (here we use [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/), you can also use others like DockerHub):

```
docker login <acr_name>.azurecr.io
docker tag chatroom <acr_name>.azurecr.io/chatroom
docker push <acr_name>.azurecr.io/chatroom
```

### Deploy to Azure Web App

First create an Azure Web App:

```
az group create --name <resource_group_name> --location CentralUS
az appservice plan create --name <plan_name> --resource-group <resource_group_name> --sku S1 --is-linux
az webapp create \
   --resource-group <resource_group_name> --plan <plan_name> --name <app_name> \
   --deployment-container-image-name nginx
```

This creates a web app with nginx image.

> This tutorial uses [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) to deploy to Azure, make sure you have Azure CLI installed first.


Then update the web app with the chat room image:

```
az webapp config container set \
   --resource-group <resource_group_name> --name <app_name> \
   --docker-custom-image-name <acr_name>.azurecr.io/chatroom \
   --docker-registry-server-url https://<acr_name>.azurecr.io \
   --docker-registry-server-user <acr_name> \
   --docker-registry-server-password <acr_password>
az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> --setting PORT=5000
az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
   --setting Azure:SignalR:ConnectionString=<connection_string>
```

Now open `https://<app_name>.azurewebsites.net` and you will see your chat room running on Azure.

> Web App now supports .NET Core 2.0, so you can directly deploy to Web App without Docker:
> 1.  Create a web app:
>     ```
>     az group create --name <resource_group_name> --location CentralUS
>     az appservice plan create --name <plan_name> --resource-group <resource_group_name> --sku S1 --is-linux
>     az webapp create \
>        --resource-group <resource_group_name> --plan <plan_name> --name <app_name> \
>        --runtime "DOTNETCORE|2.0"
>     ```
>
> 2.  Config deployment source and credential:
>     ```
>     az webapp deployment source config-local-git --resource-group <resource_group_name> --name <app_name>
>     az webapp deployment user set --user-name <user_name> --password <password>
>     ```
>
> 3.  Deploy using git:
>     ```
>     git init
>     git remote add origin <deploy_git_url>
>     git add -A
>     git commit -m "init commit"
>     git push origin master
>     ```
> 4. Update config
>     ```
>     az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> --setting PORT=5000
>     az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
>        --setting Azure:SignalR:ConnectionString=<connection_string>
>     ```

## Integrate with Azure Functions

One common scenario in real-time application is server end will produce messages and publish them to clients. In such scenario, you may use Azure function as the producer of the messages.
One benefit of using Azure function is that you can run your code on-demand without having to manage the underlying infrastructure. As a result, it's not possible to run SignalR on Azure functions, because your code only runs on-demand and cannot maintain long connections with clients.
But Azure SignalR Service, this becomes possible since the service already manages the connections for you.

Now let's see how to generate some messages using Azure functions and publishes to the chat room using the service.

The sample project can be found [here](../samples/Timer/), let's see how it works.

The core logic of the function is in [TimerFunction.cs](../samples/Timer/TimerFunction.cs):

```cs
var serviceContext = AzureSignalR.CreateServiceContext("chat");
await serviceContext.HubContext.Clients.All.SendAsync("broadcastMessage", "_BROADCAST_", $"Current time is: {DateTime.Now}");
```

You can see in the service SDK there is a `AzureSignalR.CreateServiceContext()` method that creates a `ServiceContext` object from the connection string. `ServiceContext.HubContext` implements `IHubContext<>` interface so you can access all connected clients and groups using the same API of SignalR.

In this sample we simply get the current time and broadcast it to all clients.

Then in [function.json](../samples/Timer/TimerFunction/function.json) we set the function to use timer trigger so it will get called every one minute:

```json
{
  "bindings": [
    {
      "type": "timerTrigger",
      "schedule": "0 * * * * *",
      "useMonitor": true,
      "runOnStartup": false,
      "name": "myTimer"
    }
  ],
  "disabled": false,
  "scriptFile": "../Timer.dll",
  "entryPoint": "Timer.TimerFunction.Run"
}
```

To build and deploy the Azure function, first create a function app:

```
az group create --name <resource_group_name> --location CentralUS
az storage account create --resource-group <resource_group_name> --name <storage_account_name> \
   --location CentralUS --sku Standard_LRS
az functionapp create --resource-group <resource_group_name> --name <function_name> \
   --consumption-plan-location CentralUS --storage-account <storage_account_name>
```

Then configure the deployment source to local git:

```
az functionapp deployment source config-local-git --resource-group <resource_group_name> --name <function_name>
```

In the output, you'll see the url to the git repository:

```json
{
  "url": "https://<user_name>@<deploy_git_url>"
}
```

Then config the deployment credentials:

```
az functionapp deployment user set --user-name <user_name> --password <password>
```

Then build function app:

```
nuget restore
msbuild /p:Configuration=Release
```

Deploy it using git:

```
cd bin\Release\net461
git init
git remote add origin <deploy_git_url>
git add -A
git commit -m "init commit"
git push origin master
```

Finally set the connection string to the application settings:
```
az functionapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
   --setting Azure:SignalR:ConnectionString=<connection_string>
```

Now after you log into the chat room, you'll see a broadcast of current time every one minute.

> You can see in the function code it directly calls to `ServiceContext.HubContext.Clients.All.SendAsync()`, instead of calling to `BroadcastMessage()` of the hub.
That means the call directly goes to clients through SignalR service, without need to go to the web server (which hosts the hub).
So if your application only needs to broadcast message to clients, you don't even need to host the hub logic in a web server.

In this tutorial you have learned how to use Azure services with SignalR service together to build real-time applications. So far our tutorials are based on some modern technologies like .NET Core and Azure functions, you may have existing codebase based on .NET Framework. In next tutorial you'll learn how to use SignalR service in .NET Framework so that you can still reuse your existing codebase.
