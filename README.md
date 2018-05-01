# Welcome to Azure SignalR Service

This repository contains documentation and code samples for Azure SignalR Service.

Azure SignalR Service is an Azure managed service that helps developers easily build web applications with real-time features. This service is based on [SignalR](https://github.com/aspnet/SignalR) which is an open source library that simplifies the process of adding real-time web functionality to applications.

> For more information about SignalR, please visit SignalR official [site](https://www.asp.net/signalr).

If you're new to Azure SignalR Service, this repo contains useful documentation and code samples that can help you quickly get started to use this service.

To learn how to use Azure SignalR Service, you can start with the following tutorials:

* [Get Started with SignalR: a Chat Room Example](./samples/ChatRoomLocal/README.md)
* [Build Your First Azure SignalR Service Application](./samples/ChatRoom/README.md)
* [Integrate with Azure Services](./docs/azure-integration.md)
* [Implement Your Own Authentication](./samples/GitHubChat/README.md)

More samples can be found here:

* [Realtime Sign-in Example using Azure SignalR Service](samples/RealtimeSignIn)

## Updates of Azure SignalR Service Runtime and SDK

We have made some changes to Azure SignalR Service Runtime, as well as Service SDK. You have to upgrade to the latest Service SDK to connect to the latest Service Runtime.
In the latest Service SDK, we have made the following changes:

- Dependency injection is built on top of ASP.NET Core SignalR.
- Expose a built-in authentication endpoint to issue access token. User authentication endpoint is not required any more.
- Re-design interfaces for REST API.
- Remove .NET Framework support. We plan to support .NET Framework in later releases.


> #### ASP.NET SignalR, ASP.NET Core SignalR and Azure SignalR Service
> There're two versions of SignalR: ASP.NET SignalR and ASP.NET Core SignalR. The ASP.NET Core version is not a simple .NET Core port of the original SignalR, but a [rewrite](https://blogs.msdn.microsoft.com/webdev/2017/09/14/announcing-signalr-for-asp-net-core-2-0/) of the original version.
> As a result, ASP.NET Core SignalR is not backward compatible with ASP.NET SignalR (API interfaces and behaviors are different). If you're using ASP.NET version and want to move to ASP.NET Core version, you'll need to change your code to handle these differences.
>
> Azure SignalR Service is based on ASP.NET Core SignalR, therefore you can only use ASP.NET Core SDK when using the service.
> This doesn't mean you cannot use the service in .NET Framework, our SDK is .NET Standard so you can still use it in .NET Framework, just you have to use the new APIs instead of old ones.

