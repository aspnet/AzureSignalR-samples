// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
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
            app.FullName = "Azure SignalR Management Sample: Message Publisher";
            app.HelpOption("--help");
            app.Description = "Message publisher using Azure SignalR Service Management SDK.";

            var connectionStringOption = app.Option("-c|--connectionstring", "Set connection string.", CommandOptionType.SingleValue, true);
            var serviceTransportTypeOption = app.Option("-t|--transport", "Set service transport type. Options: <transient>|<persistent>. Default value: transient. Transient: calls REST API for each message. Persistent: Establish a WebSockets connection and send all messages in the connection.", CommandOptionType.SingleValue, true);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();


            app.OnExecute(async() =>
            {
                var connectionString = connectionStringOption.Value() ?? configuration["Azure:SignalR:ConnectionString"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    MissOptions();
                    return -1;
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

                var publisher = new MessagePublisher(connectionString, serviceTransportType);
                await publisher.InitAsync();

                await StartAsync(publisher);

                return 0;
            });

            app.Execute(args);
        }

        private static async Task StartAsync(MessagePublisher publisher)
        {
            Console.CancelKeyPress += async (sender, e) =>
            {
                await publisher.DisposeAsync();
                Environment.Exit(0);
            };

            ShowHelp();

            try
            {
                while (true)
                {
                    var argLine = Console.ReadLine();
                    if (argLine == null)
                    {
                        continue;
                    }
                    var args = argLine.Split(' ');

                    if (args.Length == 2 && args[0].Equals("broadcast"))
                    {
                        Console.WriteLine($"broadcast message '{args[1]}'");
                        await publisher.SendMessages(args[0], null, args[1]);
                    }
                    else if (args.Length == 4 && args[0].Equals("send"))
                    {
                        await publisher.SendMessages(args[1], args[2], args[3]);
                        Console.WriteLine($"{args[0]} message '{args[3]}' to '{args[2]}'");
                    }
                    else if (args.Length == 4 && args[0] == "usergroup")
                    {
                        await publisher.ManageUserGroup(args[1], args[2], args[3]);
                        var preposition = args[1] == "add" ? "to" : "from";
                        Console.WriteLine($"{args[1]} user '{args[2]}' {preposition} group '{args[3]}'");
                    }
                    else
                    {
                        Console.WriteLine($"Can't recognize command {argLine}");
                    }
                }
            }
            finally
            {
                await publisher.DisposeAsync();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine(
                "*********Usage*********\n" +
                "send user <User Id> <Message>\n" +
                "send users <User Id List (Seperated by ',')> <Message>\n" +
                "send group <Group Name> <Message>\n" +
                "send groups <Group List (Seperated by ',')> <Message>\n" +
                "usergroup add <User Id> <Group Name>\n" +
                "usergroup remove <User Id> <Group Name>\n" +
                "broadcast <Message>\n" +
                "***********************");
        }

        private static void MissOptions()
        {
            Console.WriteLine("Miss required options: Connection string and Hub must be set");
        }
    }
}