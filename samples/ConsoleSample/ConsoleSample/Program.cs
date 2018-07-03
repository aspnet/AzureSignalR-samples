using System;
using Microsoft.Extensions.CommandLineUtils;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Azure SignalR Rest Sample";
            app.HelpOption("-h|--help");

            app.OnExecute(() =>
            {
                
                return 0;
            });

            var server = new ServerHandler();
            server.SendToUser("cookie-a");


        }
    }
}
