const { app } = require('@azure/functions');
const fs = require('fs').promises;
const path = require('path');

app.http('index', {
    methods: ['GET', 'POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {

        try {

            context.log(`Http function processed request for url "${request.url}"`);

            const filePath = path.join(__dirname,'../content/index.html');
            const html = await fs.readFile(filePath);

            return {
                body: html,
                headers: {
                    'Content-Type': 'text/html'
                }
            };

        } catch (error) {
            context.log(error);
            return {
                status: 500,
                jsonBody: error
            }
        }
    }
});
