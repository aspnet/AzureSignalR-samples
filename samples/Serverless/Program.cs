// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.SignalR.Samples.Serverless
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Serverless Sample";
            app.HelpOption("--help");

            var connectionStringOption = app.Option("-c|--connectionstring", "Set ConnectionString", CommandOptionType.SingleValue, true);
            var hubOption = app.Option("-h|--hub", "Set hub", CommandOptionType.SingleValue, true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .Build();

            app.Command("client", cmd =>
            {
                cmd.Description = "Start a client to listen to the service";
                cmd.HelpOption("--help");

                var userId = cmd.Argument("<userId>", "Set User ID");

                cmd.OnExecute(async () =>
                {
                    var connectionString = connectionStringOption.Value() ?? configuration["Azure:SignalR:ConnectionString"];

                    if (string.IsNullOrEmpty(connectionString) || !hubOption.HasValue())
                    {
                        MissOptions();
                        return 0;
                    }

                    var client = new ClientHandler(connectionString, hubOption.Value(), userId.Value);

                    await client.StartAsync();
                    Console.WriteLine("Client started...");
                    Console.ReadLine();
                    await client.DisposeAsync();

                    return 0;
                });
            });

            app.Command("server", cmd =>
            {
                cmd.Description = "Start a server to send message through RestAPI";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    var connectionString = connectionStringOption.Value() ?? configuration["Azure:SignalR:ConnectionString"];

                    if (string.IsNullOrEmpty(connectionString) || !hubOption.HasValue())
                    {
                        MissOptions();
                        return 0;
                    }

                    var server = new ServerHandler(connectionString, hubOption.Value());
                    await server.Start();
                    return 0;
                });
            });

            app.Execute(args);
        }

        private static void MissOptions()
        {
            Console.WriteLine("Miss required options: ConnectionString and Hub must be set");
        }
    }
}
