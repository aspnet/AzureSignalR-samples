# SignalR Service Serverless Quick Start (C#)

> [!IMPORTANT]
> Support for the Azure Functions .NET in-process model ended on November 10, 2026 ([retirement notice](https://azure.microsoft.com/en-us/updates?id=retirement-support-for-the-inprocess-model-for-net-apps-in-azure-functions-ends-10-november-2026)). For supported deployments, use the [.NET isolated worker model](https://learn.microsoft.com/azure/azure-functions/migrate-dotnet-to-isolated-model) (see [csharp-isolated](../csharp-isolated)).

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (Version 4)
* [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json`

2. Install local Azure Storage emulator [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) or update `AzureWebJobsStorage` setting to your storage account connection string.

3. Update `AzureSignalRConnectionString` setting to your SignalR Service connection string

2. In Azure Portal of your SignalR service, update Service Mode from Default to Serverless in Settings Panel.

3. Run `func start` to start Azure Function locally.

4. Visit `http://localhost:7071/api/index` and you can see the result.
