# Work with .NET Framework

Azure SignalR Service is based on SignalR Core which is based on .NET Core.
In some cases you may not want to use .NET Core because you may have an existing .NET Framework codebase.
SignalR Core itself cannot be used in .NET Framework because it has dependencies on .NET Core.
But with Azure SignalR Service, it's possible to use it in .NET Framework, because our service SDK is .NET Standard which can work in .NET Framework.

> SignalR Core has two SDKs, server SDK which hosts the SignalR runtime and client SDK which connects the client application to SignalR server.
The server part depends on .NET Core but the client part doesn't.
Since SignalR Service already hosts the SignalR server runtime for you so it already removes the server dependency. That's why it's possible to be used in .NET Framework.

> Our service SDK is a .NET Standard 2.0 library so you can only use it in .NET Framework 4.6.1 or above.

## Create an ASP.NET Chat Room

Now let's create the same chat room using ASP.NET:

1.  Create an ASP.NET Web Application (.NET Framework) in Visual Studio.

2.  Add nuget package "Microsoft.Azure.SignalR.Owin" and "Microsoft.Owin.Host.SystemWeb".

3.  Add a [Startup.cs](../samples/ChatRoomAspNet/App_Start/Startup.cs) to connect to SignalR service when startup:

    ```cs
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseAzureSignalR(ConfigurationManager.AppSettings["AzureSignalRConnectionString"],
                builder =>
                {
                    builder.UseHub<Chat>();
                });
        }
    }
    ```

4.  Add [Chat](../samples/ChatRoomAspNet/Hub/Chat.cs) hub class:

    ```cs
    public class Chat : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }
    }
    ```

5.  Add [AuthController.cs](../samples/ChatRoomAspNet/Controllers/AuthController.cs) to implement auth API:

    ```cs
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly static EndpointProvider EndpointProvider = CloudSignalR.CreateEndpointProviderFromConnectionString(ConfigurationManager.AppSettings["AzureSignalRConnectionString"]);
        private readonly static TokenProvider TokenProvider = CloudSignalR.CreateTokenProviderFromConnectionString(ConfigurationManager.AppSettings["AzureSignalRConnectionString"]);

        [HttpGet]
        [Route("{hubName}")]
        public IHttpActionResult GenerateJwtBearer(string hubName, [FromUri] string uid = null)
        {
            var serviceUrl = EndpointProvider.GetClientEndpoint(hubName);
            var accessToken = TokenProvider.GenerateClientAccessToken(hubName, new[]
            {
                new Claim(ClaimTypes.NameIdentifier, uid)
            });
            return Ok(new
            {
                ServiceUrl = serviceUrl,
                AccessToken = accessToken
            });
        }
    }
    ```

6.  Copy the same UI files (html, scripts and css) into the project folder.

The full code sample can be found [here](../samples/ChatRoomAspNet).

Now you can run it in Visual Studio.

You can also build and run it through command-line:

```
nuget restore
msbuild
%ProgramFiles(x86)%\IIS Express\iisexpress.exe" /path:<project_folder>
```

> You need to set the connection string in [web.config](../samples/ChatRoomAspNet/Web.config) when run in local:
> ```xml
> <add key="AzureSignalRConnectionString" value="<connection_string>" />
> ```

## Deploy to Azure

The we can deploy the application to Azure Web App.

1.  Create a web app:

    ```
    az group create --name <resource_group_name> --location CentralUS
    az appservice plan create --name <plan_name> --resource-group <resource_group_name> --sku S1
    az webapp create --resource-group <resource_group_name> --plan <plan_name> --name <app_name>
    ```

2.  Config deployment source and credential:

    ```
    az webapp deployment source config-local-git --resource-group <resource_group_name> --name <app_name>
    az webapp deployment user set --user-name <user_name> --password <password>
    ```

3.  Deploy using git:

    ```
    git init
    git remote add origin <deploy_git_url>
    git add -A
    git commit -m "init commit"
    git push origin master
    ```

4.  Finally set the connection string in app settings:

    ```
    az webapp config appsettings set --resource-group <resource_group_name> --name <app_name> \
       --setting AzureSignalRConnectionString=<connection_string>
    ```

Now open the site in your browser and you'll see the chat room running on Azure.
