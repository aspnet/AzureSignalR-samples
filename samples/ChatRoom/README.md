# Build Your First Azure SignalR Service Application

In [ChatRoomLocal sample](../ChatRoomLocal) you have learned how to use SignalR to build a chat room application. In that sample, the SignalR runtime (which manages the client connections and message routing) is running on your local machine. As the number of the clients increases, you'll eventually hit a limit on your machine and you'll need to scale up your machine to handle more clients. This is usually not an easy task. In this tutorial, you'll learn how to use Azure SignalR Service to offload the connection management part to the service so that you don't need to worry about the scaling problem.

## Run with aspire ready in visual studio

[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview#orchestration) is used to orchestrate the samples.

To work with .NET Aspire, you need the following installed locally:
- [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- .NET Aspire workload:
  - Installed with the [Visual Studio installer](../fundamentals/setup-tooling.md?tabs=visual-studio#install-net-aspire) or [the .NET CLI workload](../fundamentals/setup-tooling.md?tabs=dotnet-cli#install-net-aspire).
- An OCI compliant container runtime, such as:
  - [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Podman](https://podman.io/).
 
In Visual Studio, set **Samples.AppHost** project as the Startup Project. Right click **Connected Services** and select **Azure Resource Provisioning Settings** and select your Azure subscription, region and resource group to use.

Alternatively, you could add Azure related configurations in the appsettings.json file:
  ```json
  {
    "Azure": {
      "SubscriptionId": "your subscription",
      "Location": "your location"
    }
  }
  ```

Run the project and use Aspire dashboard to navigate to different samples:

![Aspire Dashboard](./images/aspire-dashboard.png)

## Run without aspire

Aspire helps you to automatically provision a new Azure SignalR resource and set the connection strings for the sample to use automatically. You could still use the traditional way to provision and set the connection strings by yourself and run the sample directly. Samples now use named connection string `AddNamedAzureSignalR("asrs1")`. Set your connection string to `Azure:SignalR:asrs1:ConnectionString`, or `ConnectionStrings:asrs1`:
### Provision a SignalR Service

First let's provision a SignalR service on Azure.
> If you don't have an Azure subscription, **[start now](https://azure.microsoft.com/en-us/free/)** to create a free account.

1. Open Azure portal, click "Create a resource" and search "SignalR Service".

   ![signalr-4](../../docs/images/signalr-4.png)

2. Navigate to "SignalR Service" and click "Create".
   
   ![signalr-5](../../docs/images/signalr-5.png)

3. Fill in basic information including resource name, resource group and location.

   ![signalr-2](../../docs/images/signalr-2.png)

   Resource name will also be used as the DNS name of your service endpoint. So you'll get a `<resource_name>.service.signalr.net` that your application can connect to.

   Select a pricing tier. There're two pricing tiers:
   
   * Free: which can handle 20 connections at the same time and can send and receive 20,000 messages in a day.
   * Standard: which has 1000 concurrent connections and one million messages per day limit for *one unit*. You can scale up to 100 units for a single service instance and you'll be charged by the number of units you use.

4. Click "Create", your SignalR service will be created in a few minutes.

   ![signalr-3](../../docs/images/signalr-3.png)

After your service is ready, go to the **Keys** page of your service instance and you'll get two connection strings that your application can use to connect to the service.

### Update Chat Room to Use Azure SignalR Service

Then, let's update the chat room sample to use the new service you just created.

Let's look at the key changes:

1.  In [Program.cs](Program.cs), call `AddNamedAzureSignalR("asrs1")` after `AddSignalR()` to make the application connect to the service instead of hosting SignalR by itself.

    ```cs
    ...
    services.AddSignalR()
            .AddNamedAzureSignalR("asrs1");
    ```

    You also need to reference the service SDK before using these APIs. This is how that would look in your ChatRoom.csproj file:

    ```xml
    <PackageReference Include="Microsoft.Azure.SignalR" Version="1.*" />
    ```

Other than these changes, everything else remains the same, you can still use the hub interface you're already familiar with to write business logic.

> Under the hood, an endpoint `/chat/negotiate` is exposed for negotiation by Azure SignalR Service SDK. It will return a special negotiation response when clients try to connect and redirect clients to service endpoint from the connection string. Read more details about the redirection at SignalR's [Negotiation Protocol](https://github.com/aspnet/SignalR/blob/master/specs/TransportProtocols.md#post-endpoint-basenegotiate-request).


Now use [dotnet dev-certs](https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs) to generate a self-signed cert to enable HTTPS use in development, and set the connection string in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=visual-studio#secret-manager) tool for .NET Core, and run this app.

```
dotnet dev-certs https --trust
dotnet user-secrets set ConnectionStrings_asrs1 "<your connection string>"
dotnet run
```

## Quick Deploy via Docker image
You can also deploy this sample via existing docker image

```
docker run -e Azure__SignalR__ConnectionString="<signalr-connection-string>" \
           -p 5000:80 mcr.microsoft.com/signalrsamples/chatroom:latest
```

When you open http://localhost:5000, you can see the application runs as usual, just instead of hosting a SignalR runtime by itself, it connects to the SignalR service running on Azure.

In this sample, you have learned how to use Azure SignalR Service to replace your self-hosted SignalR runtime. But you still need a web server to host your hub logic. In the next tutorial you'll learn how to use other Azure services to host your hub logic so you can get everything running in the cloud.
