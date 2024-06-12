# Build Your First Azure SignalR Service Application for ASP.NET SignalR

In [ChatRoomLocal sample](../ChatRoomLocal) you have learned how to use SignalR to build a chat room application. In that sample, the SignalR runtime (which manages the client connections and message routing) is running on your local machine. As the number of the clients increases, you'll eventually hit a limit on your machine and you'll need to scale up your machine to handle more clients. This is usually not an easy task. In this tutorial, you'll learn how to use Azure SignalR Service to offload the connection management part to the service so that you don't need to worry about the scaling problem.

## Provision a SignalR Service

First let's provision a SignalR service on Azure following [Quickstart: Use an ARM template to deploy Azure SignalR Service](https://learn.microsoft.com/azure/azure-signalr/signalr-quickstart-azure-signalr-service-arm-template?tabs=azure-portal).

After your service is ready, go to the **Keys** page of your service instance and you'll get the connection strings that your application can use to connect to the service.

## Update Chat Room to Use Azure SignalR Service

Then, let's update the chat room sample to use the new service you just created.

1. Use the [ChatRoomLocal sample](../ChatRoomLocal) as the starting point. If you haven't done that, you can download the sample from [here](../ChatRoomLocal/README.md).

2. Open the solution in Visual Studio 2022, select **Tools | Library Package Manager | Package Manager Console** and run command:

    ```powershell
    Install-Package Microsoft.Azure.SignalR.AspNet
    ```

3.  In [Startup.cs](Startup.cs), replace `MapSignalR()` with `MapAzureSignalR({your_applicationName})`. `{YourApplicationName}` is the unique name to distinguish this application with your other applications. You can use `this.GetType().FullName` as the value. Update [Startup.cs](Startup.cs) as below:

    ```cs
    using System;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    using Owin;

    [assembly: OwinStartup(typeof(ChatRoom.Startup))]

    namespace ChatRoom
    {
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                // Any connection or hub wire up and configuration should go here
                app.MapAzureSignalR(this.GetType().FullName);
            }
        }
    }
    ```

    Hub logic remains unchanged.

    > Under the hood, an endpoint `/signalr/negotiate` is exposed for negotiation by Azure SignalR Service SDK. It returns a special negotiation response when clients try to connect and redirects clients to connect to Azure SignalR service endpoint.

4. Now add the connection string to the `connectionStrings` section of [Web.config](Web.config), replacing `{Replace By Your Connection String}` with the connection string you copied from the Azure portal.

    ```xml
    <configuration>
        <connectionStrings>
            <add name="Azure:SignalR:ConnectionString" connectionString="{Replace By Your Connection String}"/>
        </connectionStrings>
    ...
    </configuration>
    ```

Press **F5** to run the project in debug mode. You can see the application runs as usual, just instead of hosting a SignalR runtime by itself, it connects to the SignalR service running on Azure.

> Don't forget to add assembly binding redirect in [Web.config](Web.config) if you encounter assembly loading issues.

In this sample, you have learned how to use Azure SignalR Service to replace your self-hosted SignalR runtime.
