# SignalR Service Serverless Quick Start (C#)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Function Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
* [.NET](https://dotnet.microsoft.com/download)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json`

2. Install local Azure Storage emulator [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) or update `AzureWebJobsStorage` setting to your storage account connection string.

3. Update `AzureSignalRConnectionString` setting to your SignalR Service connection string

2. In Azure Portal of your SignalR service, update Service Mode from Default to Serverless in Settings Panel.

3. Run `func start` to start Azure Function locally.

4. Visit `http://localhost:7071/api/index` and you can see the result.
