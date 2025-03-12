// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Azure.Identity;

using Microsoft.Azure.SignalR;

const string Endpoint = "https://<resourceName>.service.signalr.net";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddAzureSignalR(option =>
{
    var appTenantId = "<guid>";
    var appClientId = "<guid>";
    var clientCertificatePath = "<your path to certificate>";
    var credential = new ClientCertificateCredential(appTenantId, appClientId, clientCertificatePath);
    var endpoint = new ServiceEndpoint(new Uri(Endpoint), credential);
    option.Endpoints = [endpoint];
});
var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapHub<ChatSampleHub>("/chat");

app.Run();