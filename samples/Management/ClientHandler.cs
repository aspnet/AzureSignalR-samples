// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.SignalR.Management;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    public class ClientHandler
    {
        private readonly HubConnection _connection;

        public ClientHandler(string connectionString, string hubName, string userId)
        {
            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = connectionString;
            }).Build();

            var clientUrl = serviceManager.GetClientEndpoint(hubName);
            var accessToken = serviceManager.GenerateClientAccessToken(hubName, userId : userId);

            _connection = new HubConnectionBuilder()
                .WithUrl(clientUrl, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(accessToken);
                    };
                }).Build();

            _connection.On("SendMessage",
                (string server, string message) =>
                {
                    Console.WriteLine($"[{DateTime.Now.ToString()}] Received message from server {server}: {message}");
                });
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}