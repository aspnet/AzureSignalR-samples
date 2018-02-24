# Welcome to Azure SignalR Service

This repository contains documentation and code samples for Azure SignalR Service.

Azure SignalR Service is an Azure managed service that helps developers easily build web applications with real-time features. This service is based on [SignalR](https://github.com/aspnet/SignalR) which is an open source library that simplifies the process of adding real-time web functionality to applications.

> For more information about SignalR, please visit SignalR official [site](https://www.asp.net/signalr).

If you're new to Azure SignalR Service, this repo contains useful documentation and code samples that can help you quickly get started to use this service.

To learn how to use Azure SignalR Service, you can start with our [tutorials](tutorials/).

> #### SignalR, SignalR Core and Azure SignalR Service
> There're two versions of SignalR: SignalR (for ASP.NET) and SignalR Core (for ASP.NET Core). SignalR Core is not a simple .NET Core port of SignalR, but a [rewrite](https://blogs.msdn.microsoft.com/webdev/2017/09/14/announcing-signalr-for-asp-net-core-2-0/) of the original version.
> As a result, SignalR Core is not backward compatible with SignalR (API interfaces and behaviors are different). If you're using SignalR and want to move to SignalR Core, you'll need to change your code to handle these differences.
>
> Azure SignalR Service is based on SignalR Core, therefore you can only use SignalR Core SDK when using the service.
> This doesn't mean you cannot use the service in .NET Framework, our SDK is .NET Standard so you can still use it in .NET Framework, just you have to use the new APIs instead of old ones.
