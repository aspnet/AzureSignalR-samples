var https = require('https');

module.exports = function (context) {
    var req = https.request("https://api.github.com/repos/azure/azure-signalr", {
        method: 'GET',
        headers: {'User-Agent': 'serverless'}
    }, res => {
        var body = "";

        res.on('data', data => {
            body += data;
        });
        res.on("end", () => {
            var jbody = JSON.parse(body);
            context.bindings.signalRMessages = [{
                "target": "newMessage",
                "arguments": [ `Current star count of https://github.com/Azure/azure-signalr is: ${jbody['stargazers_count']}` ]
            }]
            context.done();
        });
    }).on("error", (error) => {
        context.log(error);
        context.res = {
          status: 500,
          body: error
        };
        context.done();
    });
    req.end();
}