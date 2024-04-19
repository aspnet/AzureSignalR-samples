const { app, output } = require('@azure/functions');
const getStars = require('../getStars');

var etag = '';
var star = 0;

const goingOutToSignalR = output.generic({
    type: 'signalR',
    name: 'signalR',
    hubName: 'serverless',
    connectionStringSetting: 'SIGNALR_CONNECTION_STRING',
});

app.timer('sendMessasge', {
    schedule: '0 * * * * *',
    extraOutputs: [goingOutToSignalR],
    handler: async (myTimer, context) => {

        try {
            const response = await getStars(etag);

            if(response.etag === etag){
                console.log(`Same etag: ${response.etag}, no need to broadcast message`);
                return;
            }
        
            etag = response.etag;
            const message = `${response.stars}`;

            context.extraOutputs.set(goingOutToSignalR,
                {
                    'target': 'newMessage',
                    'arguments': [message]
                });
        } catch (error) {
            context.log(error);
        }

    }
});
