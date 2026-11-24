# SignalR Service Serverless Quick Start (Python)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (Version 4)
* [Python 3.11+](https://www.python.org/downloads/)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json` and update `AzureSignalRConnectionString` setting to your SignalR Service connection string.

1. Run `func start` to start Azure Function locally.

1. Visit `http://localhost:7071/api/index` and you can see the result.
