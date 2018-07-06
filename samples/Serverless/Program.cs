using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Azure.SignalR.Samples.Serverless
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Serverless Sample";
            app.HelpOption("--help");

            var connectionString = app.Option("-c|--connectionstring", "Set ConnectionString", CommandOptionType.SingleValue, true);
            var hub = app.Option("-h|--hub", "Set hub", CommandOptionType.SingleValue, true);

            app.Command("client", cmd =>
            {
                cmd.Description = "Start a client to listen to the service";
                cmd.HelpOption("--help");

                var userId = cmd.Argument("<userId>", "Set User ID");

                cmd.OnExecute(async () =>
                {
                    if (!connectionString.HasValue() || !hub.HasValue())
                    {
                        MissOptions();
                        return 0;
                    }

                    var client = new ClientHandler(connectionString.Value(), hub.Value(), userId.Value);
                    await client.StartAsync();
                    Console.WriteLine("Client started...");
                    Console.ReadLine();
                    return 0;
                });
            });

            app.Command("server", cmd =>
            {
                cmd.Description = "Start a server to send message through RestAPI";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    if (!connectionString.HasValue() || !hub.HasValue())
                    {
                        MissOptions();
                        return 0;
                    }

                    var server = new ServerHandler(connectionString.Value(), hub.Value());
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
