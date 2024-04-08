# SignalR Service Serverless Quick Start (JavaScript)

In this sample, we demonstrate how to broadcast messages with SignalR Service and Azure Function in serverless.

## Prerequisites

* [Azure Function Core Tools](https://www.npmjs.com/package/azure-functions-core-tools)
* [Node.js LTS](https://nodejs.org/en/download/)

## Setup and run locally

1. Start local storage emulator.

    ```bash
    npm run start:azurite
    ```

1. Rename `local.settings.template.json` to `local.settings.json` and update env variable.

    * `SIGNALR_CONNECTION_STRING`: The connection string of your Azure SignalR Service.

1. Run command to start Azure Function locally.

    ```bash
    npm start
    ```

1. Visit `http://localhost:7071/api/index` and you can see the result.
