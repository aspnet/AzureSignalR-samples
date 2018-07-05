// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.Azure.SignalR.Samples.SimpleEcho
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var serverTask = CreateWebHostBuilder(args).Build().RunAsync(cts.Token);


            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await Task.Run(() => new Client().StartAsync(cts.Token));
            }
            finally
            {
                cts.Cancel();
                await serverTask;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
