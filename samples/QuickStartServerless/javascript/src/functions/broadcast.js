const { app } = require('@azure/functions');
const { getStars } = require('../getStars');

var etag = '';
var star = 0;

const goingOutToSignalR = output.generic({
    type: 'signalR',
    name: 'signalR',
    hubName: 'serverless',
    connectionStringSetting: 'SIGNALR_CONNECTION_STRING',
});


app.timer('sendMessasge', {
    schedule: '0 */5 * * * *',
    extraOutputs: [goingOutToSignalR],
    handler: async (myTimer, context) => {
        const response = await getStars(etag);

        etag = response.etag;

        context.extraOutputs.set(goingOutToSignalR,
            {
                'target': 'newMessage',
                'arguments': [ `Current star count of https://github.com/Azure/azure-signalr is: ${response.stars}` ]
            });

    }
});
