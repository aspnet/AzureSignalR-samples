# SignalR Service Serverless Quick Start (C#)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Function Core Tools](https://review.docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash&branch=pr-en-us-162554#v2)
* [.NET](https://dotnet.microsoft.com/download)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json`, update `AzureSignalRConnectionString` setting to your SignalR Service connection string and `AzureWebJobsStorage` setting to your stroage account connection string.

2. In Azure Portal of your SignalR service, update Service Mode from Default to Serverless in Settings Panel.

3. Run `func start` to start Azure Function locally.

4. Visit `http://localhost:7071/api/index` and you can see the result.
