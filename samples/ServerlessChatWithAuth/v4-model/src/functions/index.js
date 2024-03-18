const { app } = require('@azure/functions');
const { readFile } = require('fs/promises');

app.http('index', {
    methods: ['GET'],
    authLevel: 'anonymous',
    handler: async (context) => {
        const content = await readFile('index.html', 'utf8', (err, data) => {
            if (err) {
                context.err(err)
                return
            }
        });

        return {
            status: 200,
            headers: {
                'Content-Type': 'text/html'
            },
            body: content,
        };
    }
});