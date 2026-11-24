// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ILogger log)
        {
            var table = new SignInTable(Environment.GetEnvironmentVariable("TableConnectionString"));
            var signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

            var ua = Parser.GetDefault().Parse(req.Headers["User-Agent"].ToString());
            // add sign-in record
            await table.AddAsync(ua.OS.Family, ua.UA.Family);

            // calculate statistics
            var stats = await table.GetStatsAsync();

            // broadcast through SignalR
            await signalR.SendAsync("signinsamplehub", "updateSignInStats", stats.TotalNumber, stats.ByOS, stats.ByBrowser);

            return new OkObjectResult(new
            {
                authInfo = new
                {
                    serviceUrl = signalR.GetClientHubUrl(HubName),
                    accessToken = signalR.GenerateAccessToken(HubName)
                },
                stats = stats
            });
        }
    }
}
