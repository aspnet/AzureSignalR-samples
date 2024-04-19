# SignalR Service Serverless Quick Start (JavaScript)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Function Core Tools](https://review.docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash&branch=pr-en-us-162554#v2)
* [Node.js](https://nodejs.org/en/download/)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json` and update `AzureSignalRConnectionString` setting to your SignalR Service connection string.

1. Run `func start` to start Azure Function locally.

1. Visit `http://localhost:7071/api/index` and you can see the result.
