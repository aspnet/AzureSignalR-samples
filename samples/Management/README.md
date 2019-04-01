Azure SignalR Service Management SDK Sample 
=================================

This sample shows the use of Azure SignalR Service Management SDK.

* [Message Pulbisher](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/MessagePublisher): shows how to publish messages to SignalR clients using Management SDK.
* [Negotitation Server](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/NegotiationServer): shows how to negotiate client from you app server to Azure SignalR Service using Management SDK.
* [SignalR Client](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/SignalRClient): is a tool to start multiple SignalR clients and these clients listen messages for this sample.

## Run the Sample

### Start the Negotiation Server

```
cd NegotitationServer
setx Azure:SignalR:ConnectionString "<Your Connection String>"
dotnet run
```

## Start SignalR Client

```
cd SignalRClient
dotnet run
```

Parameters:
* -h|--hubEndpoint: Set hub endpoint. Default value: "http://localhost:5000/Management".
* -u|--user: Set user ID. Default value: "User". You can set multiple users like this: "-u user1 -u user2".

## Start Message Publisher

```
cd MessagePublisher
dotnet run
```

Parameters:
* -c|--connectionstring: Set connection string.
* -t|--transport: Set service transport type. Options: \<transient\>|\<persistent\>. Default value: transient. `Transient`: calls REST API for each message. `Persistent`: Establish a WebSockets connection and send all messages in the connection.

After the publisher started, use the command to send message

```
send user <User ID List (Seperated by ',')> <Message>
send users <User List> <Message>
send group <Group Name> <Message>
send groups <Group List (Seperated by ',')> <Message>
usergroup add <User ID> <Group Name>
usergroup remove <User ID> <Group Name>
broadcast <Message>
```

### Use user-secrets to Specify Connection String

You can run `dotnet user-secrets set Azure:SignalR:ConnectionString "<Connection String>"` in the root directory of the sample. After that, you don't need the option `-c "<Connection String>"` anymore.

## Build a Simple Message Publisher with Management SDK from Scratch

Simple **message publisher** is a console application that uses Management SDK to to publish messages to SignalR clients directly. We also need **SignalR clients** to receive mesages. To connect SignalR clients to Azure SignalR Service, we also need a **negotiation server**.

The full codes can be found here:
[Message Pulbisher](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/MessagePublisher), 
[Negotitation Server](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/NegotiationServer) and 
[SignalR Client](https://github.com/aspnet/AzureSignalR-samples/tree/master/samples/Management/SignalRClient).

Suppose you already have a connetion string. Let's implement three components step by step.

### SignalR Client
1. First create a .net core console app.
```
dotnet new console
```

2. Modify .csproj file like this:
```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
    <RootNamespace>Microsoft.Azure.SignalR.Samples.Management</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="1.1.0" />
    <PackageReference Include="Microsoft.Azure.SignalR" Version="1.0.*" />
    <PackageReference Include="Microsoft.Extensions.CommandLineUtils" Version="1.1.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="2.0.0" />
  </ItemGroup>
</Project>
```

3. Install essential nuget packages
```
dotnet restore
```

4. Modify `Program.cs`.

``` C#
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace SignalRClient
{
    class Program
    {
        private const string DefaultHubEndpoint = "http://localhost:5000/Management";
        private const string Target = "Target";
        private const string DefaultUser = "User";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample: SignalR Client Tool";
            app.HelpOption("--help");

            var hubEndpointOption = app.Option("-h|--hubEndpoint", $"Set hub endpoint. Default value: {DefaultHubEndpoint}", CommandOptionType.SingleValue, true);
            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            app.OnExecute(async () =>
            {
                var hubEndpoint = hubEndpointOption.Value() ?? DefaultHubEndpoint;
                var userIds = userIdOption.Values != null && userIdOption.Values.Count > 0 ? userIdOption.Values : new List<string>() { "User" };

                var connections = (from userId in userIds
                                   select CreateHubConnection(hubEndpoint, userId)).ToList();

                await Task.WhenAll(from conn in connections
                                   select conn.StartAsync());

                Console.WriteLine($"{connections.Count} Client(s) started...");
                Console.ReadLine();

                await Task.WhenAll(from conn in connections
                                   select conn.StopAsync());
                return 0;
            });

            app.Execute(args);
        }

        static HubConnection CreateHubConnection(string hubEndpoint, string userId)
        {
            var url = hubEndpoint.TrimEnd('/') + $"?user={userId}";
            var connection = new HubConnectionBuilder().WithUrl(url).Build();
            connection.On(Target, (string message) =>
            {
                Console.WriteLine($"{userId}: gets message from service: '{message}'");
            });
            return connection;
        }
    }
}
```

#### Brief Explanation
a. Creat `HubConnection` with hub endpoint and use `HubConnection.On` to set client method in `CreateHubConnection` method.
b. Use `HubConnection.StartAsync()` to connect a SignalR client to Azure SignalR Service.
c. Use `HubConnection.StopAsync()` to disconnect a SignalR client.


4. Run `SignalR Client`
```
dotnet run
```
Parameters described [here](###SignalR-Client).


### Negotiation Server [todo]

### Message Publisher [todo]
