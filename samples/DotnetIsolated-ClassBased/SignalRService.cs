// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetIsolated_ClassBased;

public interface IHubContextStore
{
    ServiceHubContext MessageHubContext { get; }

    // Demostrate how to use multiple hubs
    ServiceHubContext SecondHubContext { get; }
}

public class SignalRService : IHostedService, IHubContextStore
{
    private const string MessageHub = "Hub";
    private const string SecondHub = "Second";
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public ServiceHubContext MessageHubContext { get; private set; }
    public ServiceHubContext SecondHubContext { get; private set; }

    public SignalRService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        using var serviceManager = new ServiceManagerBuilder()
            .WithOptions(o => o.ConnectionString = _configuration["AzureSignalRConnectionString"])
            .WithLoggerFactory(_loggerFactory)
            .BuildServiceManager();
        MessageHubContext = await serviceManager.CreateHubContextAsync(MessageHub, cancellationToken);
        SecondHubContext = await serviceManager.CreateHubContextAsync(SecondHub, cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(Dispose(MessageHubContext), Dispose(SecondHubContext));
    }

    private static Task Dispose(ServiceHubContext hubContext)
    {
        if (hubContext == null)
        {
            return Task.CompletedTask;
        }
        return hubContext.DisposeAsync();
    }
}