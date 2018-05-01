using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using UAParser;

namespace RealtimeSignIn
{
    public class SignInInfo : TableEntity
    {
        public string OS { get; set; }
        public string Browser { get; set; }

        public SignInInfo(string os, string browser)
        {
            PartitionKey = "SignIn";
            RowKey = Guid.NewGuid().ToString();
            OS = os;
            Browser = browser;
        }

        public SignInInfo()
        {
        }
    }

    class SignInStats
    {
        public int totalNumber;
        public Dictionary<string, int> byOS = new Dictionary<string, int>();
        public Dictionary<string, int> byBrowser = new Dictionary<string, int>();
    }

    class SignInResult
    {
        public AuthInfo AuthInfo;
        public SignInStats Stats;
    }

    public static class SignInFunction
    {
        [FunctionName("signin")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                SignInHub.logger = log;
                var ua = Parser.GetDefault().Parse(req.Headers.UserAgent.ToString());
                var os = ua.OS.Family;
                var browser = ua.UserAgent.Family;
                var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TableConnectionString"));
                var client = account.CreateCloudTableClient();
                var table = client.GetTableReference("SignInInfo");
                var newInfo = new SignInInfo(os, browser);
                var insert = TableOperation.Insert(newInfo);
                table.Execute(insert);

                var query = new TableQuery<SignInInfo>();
                var stats = new SignInStats();
                foreach (var info in table.ExecuteQuery(query))
                {
                    stats.totalNumber++;
                    if (!stats.byBrowser.ContainsKey(info.Browser)) stats.byBrowser[info.Browser] = 0;
                    if (!stats.byOS.ContainsKey(info.OS)) stats.byOS[info.OS] = 0;
                    stats.byBrowser[info.Browser]++;
                    stats.byOS[info.OS]++;
                }

                var result = new SignInResult()
                {
                    AuthInfo = SignInHub.GetAuthInfo(),
                    Stats = stats
                };

                var status = await SignInHub.UpdateSignInStatsAsync(stats);
                log.Info($"Status: {status}");

                return req.CreateResponse(status, result, "application/json");
            }
            catch (Exception ex)
            {
                log.Info(ex.ToString());
                throw;
            }
        }
    }
}
