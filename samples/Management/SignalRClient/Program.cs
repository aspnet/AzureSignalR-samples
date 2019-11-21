// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace SignalRClient
{
    class Program
    {
        private const string Target = "Target";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample: SignalR Client Tool";
            app.HelpOption("--help");

            var testTypeOption = app.Option("-t|--testType",
                "Set test type: Allowed values: {Connection, Auth, Throttling}.",
                CommandOptionType.SingleValue, true);
            var unitOption = app.Option("-u|--unit", "Set Azure SignalR Service Unit", CommandOptionType.SingleValue, true);

            app.OnExecute(async () =>
            {
                if (!testTypeOption.HasValue())
                {
                    throw new ArgumentNullException(nameof(testTypeOption));
                }

                var unit = unitOption.HasValue() ? Convert.ToInt32(unitOption.Value()) : 1;
                var testType = testTypeOption.Value();

                var hubEndpoint = $"http://localhost:5678/{testType}";
                var userIds = testType == "Throttling" ? (from i in Enumerable.Range(0, unit * 1000 + 1000) select $"User_{i}").ToList(): new List<string> {"User"};

                var connections = (from userId in userIds
                                   select CreateHubConnection(hubEndpoint, userId)).ToList();

                foreach (var conn in connections)
                {
                    conn.StartAsync();
                    await Task.Delay(50);
                }

                Console.WriteLine($"{connections.Count} Client(s) started... Press Enter to exit");
                Console.ReadLine();

                await Task.WhenAll(from conn in connections
                                   select conn.StopAsync());
                Console.WriteLine($"{connections.Count} Client(s) stoped...");
                return 0;
            });

            app.Execute(args);
        }

        static HubConnection CreateHubConnection(string hubEndpoint, string userId)
        {
            var url = hubEndpoint.TrimEnd('/') + $"?user={userId}";
            var connection = new HubConnectionBuilder().WithUrl(url).Build();
            connection.On(Target, (string message) =>
            {
                Console.WriteLine($"{userId}: gets message from service: '{message}'");
            });

            connection.Closed += async ex =>
            {
                Console.WriteLine(ex);
                Environment.Exit(1);
            };

            return connection;
        }
    }
}