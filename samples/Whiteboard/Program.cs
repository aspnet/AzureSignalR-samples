// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.SignalR.Samples.Whiteboard;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Diagram>();
builder.Services.AddMvc();
builder.Services.AddSignalR().AddAzureSignalR();

var app = builder.Build();
app.UseRouting();
app.UseFileServer();
app.MapControllers();
app.MapHub<DrawHub>("/draw");

app.Run();