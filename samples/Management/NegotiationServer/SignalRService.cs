// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NegotiationServer
{
    public interface IHubContextStore
    {
        public ServiceHubContext HubContext { get; }
        public ServiceHubContext<IMessageClient> StronglyTypedHubContext { get; }
    }

    public class SignalRService : IHostedService, IHubContextStore
    {
        private const string StronglyTypedHub = "StronglyTypedHub";
        private const string Hub = "Hub";
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public ServiceHubContext HubContext { get; private set; }
        public ServiceHubContext<IMessageClient> StronglyTypedHubContext { get; private set; }

        public SignalRService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            using var serviceManager = new ServiceManagerBuilder()
                .WithConfiguration(_configuration)
                //or .WithOptions(o=>o.ConnectionString = _configuration["Azure:SignalR:ConnectionString"]
                .WithLoggerFactory(_loggerFactory)
                .BuildServiceManager();
            HubContext = await serviceManager.CreateHubContextAsync(Hub, cancellationToken);
            StronglyTypedHubContext = await serviceManager.CreateHubContextAsync<IMessageClient>(StronglyTypedHub, cancellationToken);
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            HubContext.Dispose();
            StronglyTypedHubContext.Dispose();
            return Task.CompletedTask;
        }
    }
}