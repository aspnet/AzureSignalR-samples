# Tutorial: Azure SignalR Service authentication with Azure Functions
The source code for doc: https://learn.microsoft.com/azure/azure-signalr/signalr-tutorial-authenticate-azure-functions .

## Prerequisites

* [Node.js](https://nodejs.org/en/download/) (Version 22.x or 24.x)
* [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (Version 4)

To run the code locally, please remember to remove the `userId` property of `inputSignalR` binding  in _src/negotiate.js_, as without an identity provider, the user ID of the client is missing locally.