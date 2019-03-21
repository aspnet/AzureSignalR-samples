using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace SignalRClient
{
    class Program
    {
        private const string DefaultNegotiateEndpoint = "http://localhost:5000";
        private const string Target = "Target";
        private const string Message = "Have nice day.";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Management Sample";
            app.HelpOption("--help");

            var negotiateEndpointOption = app.Option("-n|--negotiate", $"Set negotiation endpoint. Default value: {DefaultNegotiateEndpoint}", CommandOptionType.SingleValue, true);
            var userIdOption = app.Option("-u|--userIdList", "Set user ID list", CommandOptionType.MultipleValue, true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<Program>()
                .Build();

            app.OnExecute(async() =>
            {
                var negotiateEndpoint = negotiateEndpointOption.Value() ?? DefaultNegotiateEndpoint;
                var userIds = userIdOption.Value();

                var connections = (from userId in userIds select new
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

                await Task.WhenAll(from conn in connections select conn.Connection.StartAsync());

                return 0;
            });
        }
    }
}