var https = require('https');

var etag = '';
var star = 0;

module.exports = function (context) {
    var req = https.request("https://api.github.com/repos/azure/azure-signalr", {
        method: 'GET',
        headers: {'User-Agent': 'serverless', 'If-None-Match': etag}
    }, res => {
        if (res.headers['etag']) {
            etag = res.headers['etag']
        }

        var body = "";

        res.on('data', data => {
            body += data;
        });
        res.on("end", () => {
            if (res.statusCode === 200) {
                var jbody = JSON.parse(body);
                star = jbody['stargazers_count'];
            }
            
            context.bindings.signalRMessages = [{
                "target": "newMessage",
                "arguments": [ `Current star count of https://github.com/Azure/azure-signalr is: ${star}` ]
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