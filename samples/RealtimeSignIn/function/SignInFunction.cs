// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using UAParser;

namespace RealtimeSignIn
{
    public static class SignInFunction
    {
        private const string HubName = "signinsamplehub";

        [FunctionName("signin")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req, ILogger log)
        {
            var table = new SignInTable(Environment.GetEnvironmentVariable("TableConnectionString"));
            var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

            var ua = Parser.GetDefault().Parse(req.Headers.UserAgent.ToString());
            // add sign-in record
            table.Add(ua.OS.Family, ua.UA.Family);

            // calculate statistics
            var stats = table.GetStats();

            // broadcast through SignalR
            await signalR.SendAsync("signinsamplehub", "updateSignInStats", stats.TotalNumber, stats.ByOS, stats.ByBrowser);

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                authInfo = new
                {
                    serviceUrl = signalR.GetClientHubUrl(HubName),
                    accessToken = signalR.GenerateAccessToken(HubName)
                },
                stats = stats
            }, "application/json");
        }
    }
}
