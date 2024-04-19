const { app, input } = require('@azure/functions');

const inputSignalR = input.generic({
    type: 'signalRConnectionInfo',
    name: 'connectionInfo',
    hubName: 'serverless',
    connectionStringSetting: 'SIGNALR_CONNECTION_STRING',
});

app.post('negotiate', {
    authLevel: 'anonymous',
    handler: (request, context) => {
        try {
            return { body: JSON.stringify(context.extraInputs.get(inputSignalR)) }
        } catch (error) {
            context.log(error);
            return {
                status: 500,
                jsonBody: error
            }
        }
    },
    route: 'negotiate',
    extraInputs: [inputSignalR],
});
