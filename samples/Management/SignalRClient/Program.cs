// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace SignalRClient
{
    class Program
    {
        private const string MessageHubEndpoint = "http://localhost:5000/Message";
        private const string Target = "Target";
        private const string DefaultUser = "TestUser";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                FullName = "Azure SignalR Management Sample: SignalR Client Tool"
            };
            app.HelpOption("--help");

            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            app.OnExecute(async () =>
            {
                var userIds = userIdOption.Values != null && userIdOption.Values.Count > 0 ? userIdOption.Values : new List<string>() { DefaultUser };

                var connections = (from userId in userIds
                                   select CreateHubConnection(MessageHubEndpoint, userId)).ToList();

                await Task.WhenAll(from conn in connections
                                   select conn.StartAsync());

                foreach (var (connection, userId) in connections.Zip(userIds))
                {
                    Console.WriteLine($"User '{userId}' with connection id '{connection.ConnectionId}' connected.");
                }
                Console.ReadLine();

                await Task.WhenAll(from conn in connections
                                   select conn.StopAsync());
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

            connection.Closed += ex =>
            {
                Console.Write($"The connection of '{userId}' is closed.");
                //If you expect non-null exception, you need to turn on 'EnableDetailedErrors' option during client negotiation.
                if (ex != null)
                {
                    Console.Write($" Exception: {ex}");
                }
                Console.WriteLine();
                return Task.CompletedTask;
            };

            return connection;
        }
    }
}
