// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.SignalR.Samples.Whiteboard;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Diagram>();
        services.AddMvc();
        services.AddSignalR().AddAzureSignalR();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseFileServer();
        app.UseEndpoints(routes =>
        {
            routes.MapControllers();
            routes.MapHub<DrawHub>("/draw");
        });
    }
}