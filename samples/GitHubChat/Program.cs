// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

using System.Net.Http.Headers;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GitHubClientId"] ?? "";
        options.ClientSecret = builder.Configuration["GitHubClientSecret"] ?? "";
        options.Scope.Add("user:email");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = GetUserCompanyInfoAsync
        };
    });

builder.Services.AddControllers();
builder.Services.AddSignalR().AddAzureSignalR();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatSampleHub>("/chat");

app.Run();

static async Task GetUserCompanyInfoAsync(OAuthCreatingTicketContext context)
{
    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

    var response = await context.Backchannel.SendAsync(request,
        HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
    var user = await response.Content.ReadFromJsonAsync<GitHubUser>();
    if (user?.company != null)
    {
        context.Principal?.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("Company", user.company)
        }));
    }
}

class GitHubUser
{
    public string? company { get; set; }
}