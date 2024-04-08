const { app } = require('@azure/functions');
const fs = require('fs').promises;

app.http('index', {
    methods: ['GET', 'POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {

        try {

            context.log(`Http function processed request for url "${request.url}"`);

            const path = context.executionContext.functionDirectory + '../../content/index.html'
            const html = await fs.readFile(path);

            return {
                body: html,
                headers: {
                    'Content-Type': 'text/html'
                }
            };

        } catch (error) {
            context.log.error(err);
            return {
                status: 500,
                jsonBody: err
            }
        }
    }
});
