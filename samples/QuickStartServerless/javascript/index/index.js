var fs = require('fs').promises

module.exports = async function (context, req) {
    const path = context.executionContext.functionDirectory + '/../content/index.html'
    try {
        var data = await fs.readFile(path);
        context.res = {
            headers: {
                'Content-Type': 'text/html'
            },
            body: data
        }
        context.done()
    } catch (error) {
        context.log.error(err);
        context.done(err);
    }
}