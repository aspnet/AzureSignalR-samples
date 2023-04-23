using DotnetIsolated_ClassBased;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(b => b.Services
                .AddSingleton<SignalRService>()
                .AddHostedService(sp => sp.GetRequiredService<SignalRService>())
                .AddSingleton<IHubContextStore>(sp => sp.GetRequiredService<SignalRService>()))
    .Build();

host.Run();
