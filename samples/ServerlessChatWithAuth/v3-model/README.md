# Tutorial: Azure SignalR Service authentication with Azure Functions
The source code for doc: https://learn.microsoft.com/azure/azure-signalr/signalr-tutorial-authenticate-azure-functions .

To run the code locally, please remember to remove the `userId` property of `signalRConnectionInfo` binding  in _negotiate/function.json_, as without an identity provider, the user ID of the client is missing locally.