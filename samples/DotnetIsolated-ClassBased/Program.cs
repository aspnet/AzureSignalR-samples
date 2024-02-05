using DotnetIsolated_ClassBased;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(b => b.Services
    .AddServerlessHub<Functions>())
    .Build();

host.Run();
