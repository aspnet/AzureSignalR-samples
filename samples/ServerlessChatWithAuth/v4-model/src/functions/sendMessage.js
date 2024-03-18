const { app, output } = require('@azure/functions');

const signalR = output.generic({
    type: 'signalR',
    name: 'signalR',
    hubName: 'default',
    connectionStringSetting: 'AzureSignalRConnectionString',
});

app.http('messages', {
    methods: ['POST'],
    authLevel: 'anonymous',
    extraOutputs: [signalR],
    handler: async (request, context) => {
        const message = await request.json();
        message.sender = request.headers && request.headers.get('x-ms-client-principal-name') || '';

        let recipientUserId = '';
        if (message.recipient) {
            recipientUserId = message.recipient;
            message.isPrivate = true;
        }
        context.extraOutputs.set(signalR,
            {
                'userId': recipientUserId,
                'target': 'newMessage',
                'arguments': [message]
            });
    }
});
