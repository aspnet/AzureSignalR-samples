// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace SignalRClient
{
    class Program
    {
        private const string DefaultHubEndpoint = "http://localhost:5000/ManagementSampleHub";
        private const string Target = "Target";
        private const string DefaultUser = "User";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample: SignalR Client Tool";
            app.HelpOption("--help");

            var hubEndpointOption = app.Option("-h|--hubEndpoint", $"Set hub endpoint. Default value: {DefaultHubEndpoint}", CommandOptionType.SingleValue, true);
            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            app.OnExecute(async () =>
            {
                var hubEndpoint = hubEndpointOption.Value() ?? DefaultHubEndpoint;
                var userIds = userIdOption.Values != null && userIdOption.Values.Count > 0 ? userIdOption.Values : new List<string>() { "User" };

                var connections = (from userId in userIds
                                   select new StableConnection(hubEndpoint, userId)).ToList();

                await Task.WhenAll(from conn in connections
                                   select conn.StartAsync());

                Console.WriteLine($"{connections.Count} Client(s) started...");
                Console.ReadLine();

                await Task.WhenAll(from conn in connections
                                   select conn.StopAsync());
                return 0;
            });

            app.Execute(args);
        }
    }
}