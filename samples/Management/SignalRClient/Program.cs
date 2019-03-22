// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace SignalRClient
{
    class Program
    {
        private const string DefaultNegotiateEndpoint = "http://localhost:5000";
        private const string Target = "Target";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample";
            app.HelpOption("--help");

            var negotiateEndpointOption = app.Option("-n|--negotiate", $"Set negotiation endpoint. Default value: {DefaultNegotiateEndpoint}", CommandOptionType.SingleValue, true);
            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            app.OnExecute(async () =>
            {
                var negotiateEndpoint = negotiateEndpointOption.Value() ?? DefaultNegotiateEndpoint;
                var userIds = userIdOption.Values;

                var connections = (from userId in userIds
                                   select new
                                   {
                                       Connection = new HubConnectionBuilder().WithUrl(negotiateEndpoint.TrimEnd('/') + $"?user={userId}").Build(),
                                       UserId = userId
                                   }).ToList();

                foreach (var conn in connections)
                {
                    conn.Connection.On(Target, (string message) =>
                    {
                        Console.WriteLine($"{conn.UserId}: gets message from service: '{message}'");
                    });
                }

                await Task.WhenAll(from conn in connections
                                   select conn.Connection.StartAsync());

                Console.WriteLine("Client started...");
                Console.ReadLine();

                await Task.WhenAll(from conn in connections
                                   select conn.Connection.StopAsync());
                return 0;
            });

            app.Execute(args);
        }
    }
}