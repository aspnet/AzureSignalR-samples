# Welcome to Azure SignalR Service

This repository contains documentation and code samples for Azure SignalR Service.

Azure SignalR Service is an Azure managed service that helps developers easily build web applications with real-time features.

This service supports both [ASP.NET Core SignalR](https://github.com/aspnet/SignalR) and [ASP.NET SignalR](https://github.com/SignalR/SignalR). [SignalR](https://github.com/aspnet/SignalR) is an open source library that simplifies the process of adding real-time web functionality to applications. 

> The ASP.NET Core version is not a simple .NET Core port of the original SignalR, but a [rewrite](https://blogs.msdn.microsoft.com/webdev/2017/09/14/announcing-signalr-for-asp-net-core-2-0/) of the original version. As a result, [ASP.NET Core SignalR](https://github.com/aspnet/SignalR) is not backward compatible with [ASP.NET SignalR](https://github.com/SignalR/SignalR) (API interfaces and behaviors are different). If it is the first time you try SignalR, we recommend you to use the [ASP.NET Core SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR), it is **simpler, more reliable, and easier to use**.

> For more information about SignalR, please visit SignalR official [site](https://www.asp.net/signalr).

If you're new to Azure SignalR Service, this repo contains useful documentation and code samples that can help you quickly get started to use this service.

To learn how to use Azure SignalR Service, you can start with the following samples:

## ASP.NET Core SignalR samples

* [Get Started with SignalR: a Chat Room Example](samples/ChatRoomLocal)
* [Build Your First Azure SignalR Service Application](samples/ChatRoom)
* [Integrate with Azure Services](docs/azure-integration.md)
* [Implement Your Own Authentication](samples/GitHubChat)
* [Server-side Blazor](/samples/ServerSideBlazor)

More advanced samples are listed as below:

* [Realtime Sign-in Example using Azure SignalR Service](samples/RealtimeSignIn)
* [Flight Map: Realtime Monitoring Dashboard using Azure SignalR Service](samples/FlightMap)

***Archived*** Chat Room Example under AspNetCore 2.1 version:
* [Chat Room Example under AspNetCore 2.1.0](https://github.com/aspnet/AzureSignalR-samples/tree/archived/netcore21/samples/ChatRoom.NetCore21/ChatRoom/README.md)

## ASP.NET SignalR samples

* [Get Started with ASP.NET SignalR: a Chat Room Example](aspnet-samples/ChatRoomLocal)
* [Build Your First Azure SignalR Service Application for ASP.NET SignalR](aspnet-samples/ChatRoom)
