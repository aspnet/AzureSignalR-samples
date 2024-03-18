const fs = require('fs');

module.exports = async function (context, req) {
    const fileContent = fs.readFileSync('content/index.html', 'utf8');

    context.res = {
        // status: 200, /* Defaults to 200 */
        body: fileContent,
        headers: {
            'Content-Type': 'text/html'
        },
    };
}