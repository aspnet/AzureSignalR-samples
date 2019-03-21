// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample";
            app.HelpOption("--help");

            var connectionStringOption = app.Option("-c|--connectionstring", "Set connection string.", CommandOptionType.SingleValue, true);
            var serviceTransportTypeOption = app.Option("-t|--transport", "Set service transport type. Options: <transient>|<persistent>. Default value: transient. Transient: calls REST API for each message. Persistent: Establish a WebSockets connection and send all messages in the connection.", CommandOptionType.SingleValue, true); // todo: description
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .Build();

            app.Description = "Message publisher using Azure SignalR Service Management SDK.";
            app.HelpOption("--help");

            app.OnExecute(async() =>
            {
                var connectionString = connectionStringOption.Value() ?? configuration["Azure:SignalR:ConnectionString"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    MissOptions();
                    return 0;
                }

                ServiceTransportType serviceTransportType;
                if (string.IsNullOrEmpty(serviceTransportTypeOption.Value()))
                {
                    serviceTransportType = ServiceTransportType.Transient;
                }
                else
                {
                    serviceTransportType = Enum.Parse<ServiceTransportType>(serviceTransportTypeOption.Value(), true);
                }

                var server = new ServerHandler(connectionString, serviceTransportType);
                await server.InitAsync();
                await server.StartAsync();
                return 0;
            });

            app.Execute(args);
        }

        private static void MissOptions()
        {
            Console.WriteLine("Miss required options: Connection string and Hub must be set");
        }
    }
}