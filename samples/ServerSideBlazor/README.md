# Server-side Blazor with Azure SignalR Service

This sample is to show how to make Server-side Blazor work with Azure SignalR Service.

## Prerequisites
* Install [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (Version >= 6.0.3)
* Install [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) (Version >= 16.3)
> Visual Studio 2019 Preview version also works which is releasing with latest Blazor Server-side template targeting newer .Net Core version.

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

## Run the sample

Then let's update Server-side Blazor sample to use the new service you just created.

Set the connection string in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=visual-studio#secret-manager) tool for .NET Core, and run this app.

```
dotnet restore
dotnet user-secrets set Azure:SignalR:ConnectionString "<your connection string>"
dotnet run
```

After running, you will see that the web server starts, makes connections to the Azure SignalR Service instance and creates an endpoint at `https://localhost:5001/`. Browser the page and click F12, you can find the connection to Azure SignalR Service is created. See snapshot 

![serversideblazor](../../docs/images/serversideblazor.png)

> If you're blocked when visit the localhost endpoint related to `Not secure` or `This site can’t be reached`, it's caused by local cert is not trusted. Run command below to trust the dotnet built-in dev-certs before start the app.
> ```
> dotnet dev-certs https --trust
> ```

## Work with Azure Web App

From Visual Studio 2019 version 16.2.0, Azure SignalR Service is build-in web app publish process, and manage dependencies between web app and SignalR service would be much more convenient. You can experience working on local SignalR in dev local environment and working on Azure SignalR Service for Azure Web Apps at the same time without any code changes.

### Create project

In Visual Studio, choose Create a new project -> ASP.NET Core Web Application -> name project -> choose template(Blazor Server App) under ASP.NET Core 3.0. Make sure you've already installed ASP.NET Core SDK 3.0 to enable Visual Studio correctly recognize the target framework.

![serversideblazor-create](../../docs/images/serversideblazor-create.png)

Or run cmd
```
dotnet new blazorserver 
```

### Create publish profile
1. Switch to create profile first to be able to manage dependent service.
   
![serversideblazor-createprofile](../../docs/images/serversideblazor-createprofile.png)

2. Change **Advanced...** and use [**Self-Contained**](https://docs.microsoft.com/en-us/dotnet/core/deploying/#self-contained-deployments-scd) deployment mode to enable publish app with .NET Core runtime as well, cause ASP.NET Core 3 runtime is not yet officially supported in Azure Web App. You can also change deployment mode after create profile by clicking :pencil2: under summary section in the publish dashboard.

![serversideblazor-advanced](../../docs/images/serversideblazor-advanced.png)

3. Create new/select existing Azure Web App in your subscription.

### Add Azure SignalR Service dependencies

After publish profile created, you can see a warning remind you adding Azure SignalR Service dependency. Click **Add** to create new/select existing Azure SignalR service in the panel.

![serversideblazor-dependency](../../docs/images/serversideblazor-dependency.png)

### Publish the app

Then it's ready to publish. And it'll auto browser the page after publish complete. 
> It may not immediately work in the first time visiting page due to Azure Web App deployment start up latency and try refresh the page to give some delay.
> Besides, you can use browser debugger mode with F12 to validate the traffic has already redirect to Azure SignalR Service.

![serversideblazor-publish](../../docs/images/serversideblazor-publish.png)

## Enable Azure SignalR Service in local development

1. Create Server-side Blazor project.

```
dotnet new blazorserver 
```

2. Add reference to Azure SignalR SDK
   
```
dotnet add package Microsoft.Azure.SignalR --version 1.1.0
```

3. Add a call to Azure SignalR Service in [Startup.cs](Startup.cs)
   
```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddServerSideBlazor();
    services.AddSignalR().AddAzureSignalR();
    ...
}
```

4. Configure Azure SignalR Service `ConnectionString` either in [appsetting.json](appsettings.json) or with [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=visual-studio#secret-manager) tool

> Notes: Step 3 can be replaced by using hosting startup assemblies to SignalR SDK.
> 
> 1. Add configuration to turn on Azure SignalR Service in [appsetting.json](appsettings.json)
> ```js
>   "Azure": {
>     "SignalR": {
>       "Enabled": true,
>       "ConnectionString": <your-connection-string>
>     }
>   }
> ```
> 
> 2. Assign hosting startup assembly to use Azure SignalR SDK. Edit [launchSettings.json](Properties/launchSettings.json) and add a configuration like below inside `environmentVariables`.
> ```js
> "environmentVariables": {
>     ...,
>     "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.Azure.SignalR"
>   }
> ```