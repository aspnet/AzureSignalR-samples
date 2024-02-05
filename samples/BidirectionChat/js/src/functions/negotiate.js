const { app, input } = require('@azure/functions');

const inputSignalR = input.generic({
    type: 'signalRConnectionInfo',
    name: 'connectionInfo',
    hubName: 'hub',
    connectionStringSetting: 'AzureSignalRConnectionString',
    userId: '{query.x-ms-signalr-userid}'
});

app.post('negotiate', {
    authLevel: 'function',
    handler: (request, context) => {
        return { body: JSON.stringify(context.extraInputs.get(inputSignalR)) }
    },
    route: 'negotiate',
    extraInputs: [inputSignalR],
});
