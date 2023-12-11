# Build Your First Azure SignalR Service Application

In [ChatRoomLocal sample](../ChatRoomLocal) you have learned how to use SignalR to build a chat room application. In that sample, the SignalR runtime (which manages the client connections and message routing) is running on your local machine. As the number of the clients increases, you'll eventually hit a limit on your machine and you'll need to scale up your machine to handle more clients. This is usually not an easy task. In this tutorial, you'll learn how to use Azure SignalR Service to offload the connection management part to the service so that you don't need to worry about the scaling problem.

## Provision a SignalR Service

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

## Update Chat Room to Use Azure SignalR Service

Then, let's update the chat room sample to use the new service you just created.

Let's look at the key changes:

1.  In [Startup.cs](Startup.cs), call `AddAzureSignalR()` after `AddSignalR()` and pass in connection string to make the application connect to the service instead of hosting SignalR by itself.

    ```cs
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddSignalR()
                .AddAzureSignalR();
    }
    ```

    You also need to reference the service SDK before using these APIs. This is how that would look in your ChatRoom.csproj file:

    ```xml
    <PackageReference Include="Microsoft.Azure.SignalR" Version="1.*" />
    ```

Other than these changes, everything else remains the same, you can still use the hub interface you're already familiar with to write business logic.

> Under the hood, an endpoint `/chat/negotiate` is exposed for negotiation by Azure SignalR Service SDK. It will return a special negotiation response when clients try to connect and redirect clients to service endpoint from the connection string. Read more details about the redirection at SignalR's [Negotiation Protocol](https://github.com/aspnet/SignalR/blob/master/specs/TransportProtocols.md#post-endpoint-basenegotiate-request).


Now set the connection string in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=visual-studio#secret-manager) tool for .NET Core, and run this app.

```
dotnet restore
dotnet user-secrets set Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

When you open http://localhost:5000, you can see the application runs as usual, just instead of hosting a SignalR runtime by itself, it connects to the SignalR service running on Azure.

In this sample, you have learned how to use Azure SignalR Service to replace your self-hosted SignalR runtime. But you still need a web server to host your hub logic. In the next tutorial you'll learn how to use other Azure services to host your hub logic so you can get everything running in the cloud.
