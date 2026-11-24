var fs = require('fs').promises

module.exports = async function (context, req) {
    const path = context.executionContext.functionDirectory + '/../content/index.html'
    var data = await fs.readFile(path);
    context.res = {
        headers: {
            'Content-Type': 'text/html'
        },
        body: data
    }
}