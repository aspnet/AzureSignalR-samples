// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Azure.SignalR.Management;

namespace AuthRefreshSample.Serverless.Management;

internal sealed class SignalRService : IHostedService
{
    public const string HubName = "chat";

    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public SignalRService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public ServiceHubContext HubContext { get; private set; } = null!;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceManager = new ServiceManagerBuilder()
            .WithConfiguration(_configuration)
            .WithLoggerFactory(_loggerFactory)
            .BuildServiceManager();

        HubContext = await serviceManager.CreateHubContextAsync(HubName, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        HubContext?.DisposeAsync() ?? Task.CompletedTask;
}