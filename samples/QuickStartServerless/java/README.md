# SignalR Service Serverless Quick Start (Java)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Function Core Tools](https://review.docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash&branch=pr-en-us-162554#v2)
* [Java version 11](https://www.azul.com/downloads/zulu/)

## Setup and run locally

1. Rename `local.settings.template.json` to `local.settings.json` and update `AzureSignalRConnectionString` setting to your SignalR Service connection string.

1. Run the following command to start Azure Function locally.

    ```bash
    mvn clean package
    mvn azure-functions:run
    ```

1. Visit `http://localhost:7071/api/index` and you can see the result.
