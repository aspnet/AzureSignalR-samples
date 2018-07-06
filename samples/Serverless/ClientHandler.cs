using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.SignalR.Sample.ConsoleSample;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Azure.SignalR.Samples.Serverless
{
    public class ClientHandler
    {
        private readonly HubConnection _connection;

        public ClientHandler(string connectionString, string hubName, string userId)
        {
            var serviceUtils = new ServiceUtils(connectionString);

            var url = GetClientUrl(serviceUtils.Endpoint, hubName);

            _connection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(serviceUtils.GenerateAccessToken(url, userId));
                    };
                }).Build();

            _connection.On<string, string>("SendMessage",
                (string server, string message) =>
                {
                    Console.WriteLine($"Received message from server {server}: {message}");
                });
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        private string GetClientUrl(string endpoint, string hubName)
        {
            return $"{endpoint}:5001/client/?hub={hubName}";
        }
    }
}