// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Azure.Core;
using Azure.Identity;

using Microsoft.Azure.SignalR;

const string Endpoint = "https://testresource.service.signalr.net";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddAzureSignalR(option =>
{
    var appTenantId = Guid.NewGuid().ToString();
    var appClientId = Guid.NewGuid().ToString();
    var msiClientId = Guid.NewGuid().ToString();

    var msiCredential = new ManagedIdentityCredential(msiClientId);

    var credential = new ClientAssertionCredential(appTenantId, appClientId, async(ctoken) =>
    {
        // Entra ID US Government: api://AzureADTokenExchangeUSGov
        // Entra ID China operated by 21Vianet: api://AzureADTokenExchangeChina
        var request = new TokenRequestContext([$"api://AzureADTokenExchange/.default"]);
        var response = await msiCredential.GetTokenAsync(request, ctoken).ConfigureAwait(false);
        return response.Token;
    });
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
