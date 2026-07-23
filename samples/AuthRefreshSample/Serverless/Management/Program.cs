// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;
using System.Text;

using AuthRefreshSample;
using AuthRefreshSample.Serverless.Management;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.SignalR.Management;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

var serviceTokenLifetime = TimeSpan.FromHours(1);
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DemoAuth.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = DemoAuth.Issuer,
            ValidateAudience = true,
            ValidAudience = DemoAuth.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.SecurityToken is JsonWebToken jwt && jwt.ValidTo > DateTime.UtcNow)
                {
                    context.Properties.ExpiresUtc = new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero);
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<SignalRService>();
builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<SignalRService>());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/chat/negotiate", async (HttpContext httpContext, SignalRService signalR) =>
{
    var authentication = await httpContext.AuthenticateAsync();
    var expiresAt = authentication.Properties?.ExpiresUtc;
    var userId = GetUserId(httpContext.User);
    if (expiresAt <= DateTimeOffset.UtcNow || string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var result = await signalR.HubContext.NegotiateWithTokenLifetimeAsync(
        new NegotiationOptions
        {
            HttpContext = httpContext,
            UserId = userId,
            Claims = BuildClaims(userId),
            TokenLifetime = serviceTokenLifetime,
            AuthenticationExpiresOn = expiresAt,
            EnableAuthenticationRefresh = true,
            CloseOnAuthenticationExpiration = true,
        },
        httpContext.RequestAborted);

    return Results.Json(new
    {
        url = result.Url,
        accessToken = result.AccessToken,
        tokenLifetimeSeconds = result.TokenLifetimeSeconds,
    });
}).RequireAuthorization();

app.MapPost("/chat/refresh", async (HttpContext httpContext, SignalRService signalR) =>
{
    var connectionToken = httpContext.Request.Query["id"].FirstOrDefault();
    if (string.IsNullOrEmpty(connectionToken))
    {
        return Results.BadRequest(new { error = "missing_connection_token" });
    }

    var authentication = await httpContext.AuthenticateAsync();
    var expiresAt = authentication.Properties?.ExpiresUtc;
    var userId = GetUserId(httpContext.User);
    if (expiresAt <= DateTimeOffset.UtcNow || string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var result = await signalR.HubContext.RefreshConnectionAuthenticationAsync(
        connectionToken,
        expiresAt,
        BuildClaims(userId),
        httpContext.RequestAborted);

    return Results.Json(new
    {
        accessToken = result.AccessToken,
        tokenLifetimeSeconds = result.TokenLifetimeSeconds,
    });
}).RequireAuthorization();

app.Run();

static string? GetUserId(ClaimsPrincipal user) =>
    user.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

static List<Claim> BuildClaims(string userId) =>
    [new Claim(ClaimTypes.NameIdentifier, userId)];