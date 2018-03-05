# Integrate with Azure Services

In last tutorial you have learned how to use Azure SignalR Service in your application, but you still need to host the hub implementation on a web server.
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
docker run -p 5000:5000 -e AzureSignalRConnectionString=<connection_string> chatroom
```

> For more information about building Docker image for .NET Core, please refer to this [doc](https://docs.microsoft.com/en-us/dotnet/core/docker/building-net-docker-images).

After you test the image, push it to a Docker register (here we use [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/), you can also use others like DockerHub):

```
docker login <acr_name>.azurecr.io
docker tag chatdemo <acr_name>.azurecr.io/chatdemo
docker push <acr_name>.azurecr.io/chatdemo
```

### Deploy to Azure Web App

First create an Azure Web App:

```
az group create --name <resource_group_name> --location "East US"
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
az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> --setting AzureSignalRConnectionString=<connection_string>
```

Now open `http://<app_name>.azurewebsites.net` and you will see your chat room running on Azure.

> Since https is not supported on SignalR yet, it's required to use http to access the web site (and chrome has some issues in sending data over non secure web site, you have to use Edge for now). This issue should be gone after https support is added.

## Integrate with Azure Functions