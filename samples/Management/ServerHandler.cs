// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    public class ServerHandler
    {
        private const string Target = "SendMessage";

        private const string Message = "Hello from server";

        private readonly string _serverName;

        private readonly string _hubName;

        private readonly string _connectionString;

        private readonly ServiceTransportType _serviceTransportType;

        private IServiceHubContext _hubContext;

        public ServerHandler(string connectionString, string hubName, ServiceTransportType serviceTransportType)
        {
            _connectionString = connectionString;
            _hubName = hubName;
            _serviceTransportType = serviceTransportType;

            _serverName = GenerateServerName();

            Console.CancelKeyPress += (sender, e) =>
            {
                _hubContext?.DisposeAsync();
                Environment.Exit(0);
            };
        }

        public async Task InitAsync()
        {
            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = _connectionString;
                option.ServiceTransportType = _serviceTransportType;
            }).Build();

            _hubContext = await serviceManager.CreateHubContextAsync(_hubName);
        }

        public async Task StartAsync()
        {
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

                    if (args.Length == 1 && args[0].Equals("broadcast"))
                    {
                        Console.WriteLine($"{Target} {_serverName} {Message}");
                        await _hubContext.Clients.All.SendAsync(Target, _serverName, Message);
                    }
                    else if (args.Length == 3 && args[0].Equals("send"))
                    {
                        await SendMessages(args[1], args[2]);
                    }
                    else if (args.Length == 4 && args[0] == "usergroup")
                    {
                        await ManageUserGroup(args[1], args[2], args[3]);
                    }
                    else
                    {
                        Console.WriteLine($"Can't recognize command {argLine}");
                    }
                }
            }
            finally
            {
                await _hubContext.DisposeAsync();
            }
        }

        private Task ManageUserGroup(string command, string userId, string groupName)
        {
            switch (command)
            {
                case "add":
                    return _hubContext.UserGroups.AddToGroupAsync(userId, groupName);
                case "remove":
                    return _hubContext.UserGroups.RemoveFromGroupAsync(userId, groupName);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        private Task SendMessages(string command, string parameter)
        {
            switch (command)
            {
                case "user":
                    var userId = parameter;
                    return _hubContext.Clients.User(userId).SendAsync(Target, _serverName, Message);
                case "users":
                    var userIds = parameter.Split(',');
                    return _hubContext.Clients.Users(userIds).SendAsync(Target, _serverName, Message);
                case "group":
                    var groupName = parameter;
                    return _hubContext.Clients.Group(groupName).SendAsync(Target, _serverName, Message);
                case "groups":
                    var groupNames = parameter.Split(',');
                    return _hubContext.Clients.Groups(groupNames).SendAsync(Target, _serverName, Message);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        private static string GenerateServerName()
        {
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private static void ShowHelp()
        {
            Console.WriteLine("*********Usage*********\n" +
                              "send user <User Id>\n" +
                              "send users <User Id List (Seperate with ',')>\n" +
                              "send group <Group Name>\n" +
                              "send groups <Group List (Seperate with ',')>\n" +
                              "usergroup add <User Id> <Group Name>\n" +
                              "usergroup remove <User Id> <Group Name>\n" +
                              "broadcast\n" +
                              "***********************");
        }
    }
}
