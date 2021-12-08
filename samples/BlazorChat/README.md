# Build Blazor Server chat app

This tutorial shows you how to build and modify a Blazor Server app. You'll learn how to:

> **&#x2713;** Build a simple chat room with Blazor Server app.
>
> **&#x2713;** Modify Razor components.
> 
> **&#x2713;** Use event handling and data binding in components.
> 
> **&#x2713;** Quick deploy to Azure App Service in Visual Studio.
> 
> **&#x2713;** Migrate local SignalR to Azure SignalR Service.

## Prerequisites
* Install [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0) (Version >= 3.0.100)
* Install [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) (Version >= 16.3)
> Visual Studio 2019 Preview version also works which is releasing with latest Blazor Server app template targeting newer .Net Core version.

## Build a local chat room in Blazor Server app

From Visual Studio 2019 version 16.2.0, Azure SignalR Service is build-in web app publish process, and manage dependencies between web app and SignalR service would be much more convenient. You can experience working on local SignalR in dev local environment and working on Azure SignalR Service for Azure App Service at the same time without any code changes.

1. Create a chat Blazor app
   
   In Visual Studio, choose Create a new project -> Blazor App -> (name the app and choose a folder) -> Blazor Server App. Make sure you've already installed .NET Core SDK 3.0+ to enable Visual Studio correctly recognize the target    framework.
   
   ![serversideblazor-create](../../docs/images/serversideblazor-create.png)
   
   Or run cmd
   ```dotnetcli
   dotnet new blazorserver -o BlazorChat
   ```
   
2. Add a `BlazorChatSampleHub.cs` file to implement `Hub` for chat.
   
   ```cs
   using System;
   using System.Threading.Tasks;
   using Microsoft.AspNetCore.SignalR;
   
   namespace BlazorChat
   {
       public class BlazorChatSampleHub : Hub
       {
           public const string HubUrl = "/chat";
   
           public async Task Broadcast(string username, string message)
           {
               await Clients.All.SendAsync("Broadcast", username, message);
           }
   
           public override Task OnConnectedAsync()
           {
               Console.WriteLine($"{Context.ConnectionId} connected");
               return base.OnConnectedAsync();
           }
   
           public override async Task OnDisconnectedAsync(Exception e)
           {
               Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
               await base.OnDisconnectedAsync(e);
           }
       }
   }
   ```
   
3. Add an endpoint for the hub in `Startup.Configure()`.
   
   ```cs
   app.UseEndpoints(endpoints =>
   {
       endpoints.MapBlazorHub();
       endpoints.MapFallbackToPage("/_Host");
       endpoints.MapHub<BlazorChatSampleHub>(BlazorChatSampleHub.HubUrl);
   });
   ```
   
4. Install `Microsoft.AspNetCore.SignalR.Client` package to use SignalR client.

   ```cmd
   dotnet add package Microsoft.AspNetCore.SignalR.Client --version 3.1.7
   ```

5. Create `ChartRoom.razor` under `Pages` folder to implement SignalR client. Follow steps below or simply copy the [ChatRoom.razor](Pages/ChatRoom.razor).

   1. Add page link and reference
      
      ```razor
      @page "/chatroom"
      @inject NavigationManager navigationManager
      @using Microsoft.AspNetCore.SignalR.Client;
      ```

   2. Add code to new SignalR client to send and receive messages.
      
      ```razor
      @code {
          // flag to indicate chat status
          private bool _isChatting = false;
          
          // name of the user who will be chatting
          private string _username;
      
          // on-screen message
          private string _message;
          
          // new message input
          private string _newMessage;
          
          // list of messages in chat
          private List<Message> _messages = new List<Message>();
          
          private string _hubUrl;
          private HubConnection _hubConnection;
      
          public async Task Chat()
          {
              // check username is valid
              if (string.IsNullOrWhiteSpace(_username))
              {
                  _message = "Please enter a name";
                  return;
              };
      
              try
              {
                  // remove old messages if any
                  _messages.Clear();
         
                  // Create the chat client
                  string baseUrl = navigationManager.BaseUri;
      
                  _hubUrl = baseUrl.TrimEnd('/') + BlazorChatSampleHub.HubUrl;
      
                  _hubConnection = new HubConnectionBuilder()
                      .WithUrl(_hubUrl)
                      .Build();
      
                  _hubConnection.On<string, string>("Broadcast", BroadcastMessage);
      
                  await _hubConnection.StartAsync();
      
                  _isChatting = true;
      
                  await SendAsync($"[Notice] {_username} joined chat room.");
              }
              catch (Exception e)
              {
                  _message = $"ERROR: Failed to start chat client: {e.Message}";
              }
          }
      
          private void BroadcastMessage(string name, string message)
          {
              bool isMine = name.Equals(_username, StringComparison.OrdinalIgnoreCase);
      
              _messages.Add(new Message(name, message, isMine));
      
              // Inform blazor the UI needs updating
              StateHasChanged();
          }
      
          private async Task DisconnectAsync()
          {
              if (_isChatting)
              {
                  await SendAsync($"[Notice] {_username} left chat room.");
      
                  await _hubConnection.StopAsync();
                  await _hubConnection.DisposeAsync();
      
                  _hubConnection = null;
                  _isChatting = false;
              }
          }
      
          private async Task SendAsync(string message)
          {
              if (_isChatting && !string.IsNullOrWhiteSpace(message))
              {
                  await _hubConnection.SendAsync("Broadcast", _username, message);
      
                  _newMessage = string.Empty;
              }
          }
      
          private class Message
          {
              public Message(string username, string body, bool mine)
              {
                  Username = username;
                  Body = body;
                  Mine = mine;
              }
      
              public string Username { get; set; }
              public string Body { get; set; }
              public bool Mine { get; set; }
      
              public bool IsNotice => Body.StartsWith("[Notice]");
      
              public string CSS => Mine ? "sent" : "received";
          }
      }
      ```

   3. Add rendering part before `@code` for UI to interact with SignalR client.

      ```razor
      <h1>Blazor SignalR Chat Sample</h1>
      <hr />
      
      @if (!_isChatting)
      {
          <p>
              Enter your name to start chatting:
          </p>
      
          <input type="text" maxlength="32" @bind="@_username" />
          <button type="button" @onclick="@Chat"><span class="oi oi-chat" aria-hidden="true"></span> Chat!</button>
      
          // Error messages
          @if (_message != null)
          {
              <div class="invalid-feedback">@_message</div>
              <small id="emailHelp" class="form-text text-muted">@_message</small>
          }
      }
      else
      {
          // banner to show current user
          <div class="alert alert-secondary mt-4" role="alert">
              <span class="oi oi-person mr-2" aria-hidden="true"></span>
              <span>You are connected as <b>@_username</b></span>
              <button class="btn btn-sm btn-warning ml-md-auto" @onclick="@DisconnectAsync">Disconnect</button>
          </div>
          // display messages
          <div id="scrollbox">
              @foreach (var item in _messages)
              {
                  @if (item.IsNotice)
                  {
                      <div class="alert alert-info">@item.Body</div>
                  }
                  else
                  {
                      <div class="@item.CSS">
                          <div class="user">@item.Username</div>
                          <div class="msg">@item.Body</div>
                      </div>
                  }
              }
              <hr />
              <textarea class="input-lg" placeholder="enter your comment" @bind="@_newMessage"></textarea>
              <button class="btn btn-default" @onclick="@(() => SendAsync(_newMessage))">Send</button>
          </div>
      }
      ```

6. Update `NavMenu.razor` to insert a entry menu for the chat room under `NavMenuCssClass` like rest.

   ```razor
   <li class="nav-item px-3">
       <NavLink class="nav-link" href="chatroom">
           <span class="oi oi-chat" aria-hidden="true"></span> Chat room
       </NavLink>
   </li>
   ```
   
7. Update `site.css` to optimize for chart area bubble views. Append below code in the end.

   ```css
   /* improved for chat text box */
   textarea {
       border: 1px dashed #888;
       border-radius: 5px;
       width: 80%;
       overflow: auto;
       background: #f7f7f7
   }
   
   /* improved for speech bubbles */
   .received, .sent {
       position: relative;
       font-family: arial;
       font-size: 1.1em;
       border-radius: 10px;
       padding: 20px;
       margin-bottom: 20px;
   }
   
   .received:after, .sent:after {
       content: '';
       border: 20px solid transparent;
       position: absolute;
       margin-top: -30px;
   }
   
   .sent {
       background: #03a9f4;
       color: #fff;
       margin-left: 10%;
       top: 50%;
       text-align: right;
   }
   
   .received {
       background: #4CAF50;
       color: #fff;
       margin-left: 10px;
       margin-right: 10%;
   }
   
   .sent:after {
       border-left-color: #03a9f4;
       border-right: 0;
       right: -20px;
   }
   
   .received:after {
       border-right-color: #4CAF50;
       border-left: 0;
       left: -20px;
   }
   
   /* div within bubble for name */
   .user {
       font-size: 0.8em;
       font-weight: bold;
       color: #000;
   }
   
   .msg {
       /*display: inline;*/
   }
   ```

8. Click <kbd>F5</kbd> to run the app. You'll be able to chat like below.

   ![blazorchat](../../docs/images/blazorchat.gif)

## Publish to Azure App Service and migrate to Azure SignalR Service

   So far, the Blazor App is working on local SignalR and when deploy to Azure App Service, it's suggested to use [Azure SignalR Service](https://docs.microsoft.com/en-us/aspnet/core/signalr/scale?view=aspnetcore-3.1#azure-signalr-service) which allows for scaling up a Blazor Server app to a large number of concurrent SignalR connections. In addition, the SignalR service's global reach and high-performance data centers significantly aid in reducing latency due to geography.

> IMPORTANT
> 
> In Blazor Server app, UI states are maintained at server side which means server sticky is required in this case. If there's single app server, server sticky is ensured by design. However, if there're multiple app servers, there's a chance that client negotiation and connection may go to different servers and leads to UI errors in Blazor app. So you need to enable server sticky like below in `appsettings.json`:
> ```json
> "Azure:SignalR:ServerStickyMode": "Required"
> ```

1. Right click the project and navigate to `Publish`.

   * Target: Azure
   * Specific target: All types of **Azure App Service** are supported.
   * App Service: create a new one or select existing app service.

   ![blazorchat-profile](../../docs/images/blazorchat-profile.gif)

2. Add Azure SignalR Service dependency

   After publish profile created, you can see a recommended message under **Service Dependencies**. Click **Configure** to create new or select existing Azure SignalR Service in the panel.

   ![serversideblazor-dependency](../../docs/images/serversideblazor-dependency.png)

   The service dependency will do things below to enable your app automatically switch to Azure SignalR Service when on Azure:

   * Update [`HostingStartupAssembly`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-3.1) to use Azure SignalR Service.
   * Add Azure SignalR Service Nuget package reference.
   * Update profile properties to save the dependency settings.
   * Configure secrets store depends on your choice.
   * Add `appsettings` configuration to make your app target selected Azure SignalR Service.

    ![serversideblazor-dependencysummary](../../docs/images/serversideblazor-dependencysummary.png)

3. Publish the app

   Now it's ready to publish. And it'll auto browser the page after publishing completes. 
   > It may not immediately work in the first time visiting page due to Azure Web App deployment start up latency and try refresh the page to give some delay.
   > Besides, you can use browser debugger mode with <kbd>F12</kbd> to validate the traffic has already redirect to Azure SignalR Service.

   ![blazorchat-azure](../../docs/images/blazorchat-azure.png)

## Further topic: Enable Azure SignalR Service in local development

1. Add reference to Azure SignalR SDK

   ```cmd
   dotnet add package Microsoft.Azure.SignalR --version 1.5.1
   ```

2. Add a call to Azure SignalR Service in in `Startup.ConfigureServices()`.

   ```cs
   public void ConfigureServices(IServiceCollection services)
   {
       ...
       services.AddSignalR().AddAzureSignalR();
       ...
   }
   ```

3. Configure Azure SignalR Service `ConnectionString` either in [appsetting.json](appsettings.json) or with [Secret Manager](https://docs.microsoft.com/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=visual-studio#secret-manager) tool.

> NOTE
> 
> Step 2 can be replaced by using [`HostingStartupAssembly`](https://docs.microsoft.com/aspnet/core/fundamentals/host/platform-specific-configuration?view=aspnetcore-3.1) to SignalR SDK.
> 
> 1. Add configuration to turn on Azure SignalR Service in [appsetting.json](appsettings.json)
>    ```js
>      "Azure": {
>        "SignalR": {
>          "Enabled": true,
>          "ServerStickyMode": "Required",
>          "ConnectionString": <your-connection-string>
>        }
>      }
>    ```
> 
> 2. Assign hosting startup assembly to use Azure SignalR SDK. Edit [launchSettings.json](Properties/launchSettings.json) and add a configuration like below inside `environmentVariables`.
>    ```js
>    "environmentVariables": {
>        ...,
>        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.Azure.SignalR"
>      }
>    ```