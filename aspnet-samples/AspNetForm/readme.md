# Azure SignalR Service for Asp.Net Form

This is a sample project for Azure SignalR Service for Asp.Net Form.
It is a simple chat application.
It is based on the [Azure SignalR Service](https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-overview) and [Azure SignalR Service SDK for .NET](https://github.com/Azure/azure-signalr).

## How to write this project

1. Create a new Asp.Net Form project.
1. Add the [Microsoft.Azure.SignalR.AspNet](https://www.nuget.org/packages/Microsoft.Azure.SignalR.AspNet/) package.	
1. Add the other necessary packages:
	* Microsoft.Owin.Cors
	* Microsoft.Owin.Host.SystemWeb
1. Add following classes:	* Startup.cs	* NotificationItem.cs	* NotificationHub.cs	* Database.cs	* Content\\my.css	* Scripts\\my.js1. Update Default.aspx.1. Add GroupOne.aspx.1. Update Site.Master.1. Remove useless pages. ## Run this project1. Privision an instance of Azure SignalR Service.1. Open `web.config`, update the connection string for Azure SignalR Service.1. Run it.