﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    public class MessagePublisher
    {
        public const string Message = "Hello from server";
        private const string Target = "Target";
        private const string HubName = "Management";
        private readonly string _connectionString;
        private readonly ServiceTransportType _serviceTransportType;
        private IServiceHubContext _hubContext;

        public MessagePublisher(string connectionString, ServiceTransportType serviceTransportType)
        {
            _connectionString = connectionString;
            _serviceTransportType = serviceTransportType;
        }

        public async Task InitAsync()
        {
            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = _connectionString;
                option.ServiceTransportType = _serviceTransportType;
            }).Build();

            _hubContext = await serviceManager.CreateHubContextAsync(HubName);
        }

        public Task ManageUserGroup(string command, string userId, string groupName)
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

        public Task SendMessages(string command, string receiver, string message)
        {
            switch (command)
            {
                case "broadcast":
                    return _hubContext.Clients.All.SendAsync(Target, message);
                case "user":
                    var userId = receiver;
                    return _hubContext.Clients.User(userId).SendAsync(Target, message);
                case "users":
                    var userIds = receiver.Split(',');
                    return _hubContext.Clients.Users(userIds).SendAsync(Target, message);
                case "group":
                    var groupName = receiver;
                    return _hubContext.Clients.Group(groupName).SendAsync(Target, message);
                case "groups":
                    var groupNames = receiver.Split(',');
                    return _hubContext.Clients.Groups(groupNames).SendAsync(Target, message);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        public Task DisposeAsync() => _hubContext?.DisposeAsync();
    }
}