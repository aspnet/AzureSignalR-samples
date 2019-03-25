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
        private const string DefaultHubEndpoint = "http://localhost:5000/Management";
        private const string Target = "Target";
        private const string DefaultUser = "User";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample";
            app.HelpOption("--help");

            var hubEndpointOption = app.Option("-h|--hubEndpoint", $"Set hub endpoint. Default value: {DefaultHubEndpoint}", CommandOptionType.SingleValue, true);
            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            app.OnExecute(async () =>
            {
                var hubEndpoint = hubEndpointOption.Value() ?? DefaultHubEndpoint;
                var userIds = userIdOption.Values != null && userIdOption.Values.Count() > 0 ? userIdOption.Values : new List<string>() { "User" };

                await Task.WhenAll(from userId in userIds
                                   select EstablishHubConnection(hubEndpoint, userId));

                Console.WriteLine($"{userIds.Count} Client(s) started...");
                Console.ReadLine();
                return 0;
            });

            app.Execute(args);
        }

        static Task EstablishHubConnection(string hubEndpoint, string userId)
        {
            var url = hubEndpoint.TrimEnd('/') + $"?user={userId}";
            var connection = new HubConnectionBuilder().WithUrl(url).Build();
            connection.On(Target, (string message) =>
            {
                Console.WriteLine($"{userId}: gets message from service: '{message}'");
            });
            return connection.StartAsync();
        }
    }
}