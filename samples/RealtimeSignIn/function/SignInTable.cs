// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RealtimeSignIn
{
    class SignInTable
    {
        public class SignInStats
        {
            [JsonProperty(PropertyName = "totalNumber")]
            public int TotalNumber { get; set; }

            [JsonProperty(PropertyName = "byOS")]
            public Dictionary<string, int> ByOS { get; private set; }

            [JsonProperty(PropertyName = "byBrowser")]
            public Dictionary<string, int> ByBrowser { get; private set; }

            public SignInStats()
            {
                ByOS = new Dictionary<string, int>();
                ByBrowser = new Dictionary<string, int>();
            }
        }

        class SignInInfo : TableEntity
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

        private CloudTable table;

        public SignInTable(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            table = client.GetTableReference("SignInInfo");
        }

        public void Add(string os, string browser)
        {
            var newInfo = new SignInInfo(os, browser);
            var insert = TableOperation.Insert(newInfo);
            table.Execute(insert);
        }

        public SignInStats GetStats()
        {
            var query = new TableQuery<SignInInfo>();
            var stats = new SignInStats();
            foreach (var info in table.ExecuteQuery(query))
            {
                stats.TotalNumber++;
                if (!stats.ByBrowser.ContainsKey(info.Browser)) stats.ByBrowser[info.Browser] = 0;
                if (!stats.ByOS.ContainsKey(info.OS)) stats.ByOS[info.OS] = 0;
                stats.ByBrowser[info.Browser]++;
                stats.ByOS[info.OS]++;
            }
            return stats;
        }
    }
}
