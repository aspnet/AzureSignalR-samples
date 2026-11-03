// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Security.Claims;
using System.Text;

using AuthRefreshSample;
using AuthRefreshSample.Serverless.Management;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.SignalR.Common;
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
    var role = GetRole(httpContext.User);
    if (expiresAt <= DateTimeOffset.UtcNow || string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var result = await signalR.HubContext.NegotiateWithTokenLifetimeAsync(
        new NegotiationOptions
        {
            HttpContext = httpContext,
            UserId = userId,
            Claims = BuildClaims(userId, role),
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
    var role = GetRole(httpContext.User);
    if (expiresAt <= DateTimeOffset.UtcNow || string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    if (httpContext.User.IsInRole("blocked"))
    {
        return Results.Json(
            new { error = "permission_change_rejected" },
            statusCode: StatusCodes.Status403Forbidden);
    }

    try
    {
        var result = await signalR.HubContext.RefreshConnectionAuthenticationAsync(
            connectionToken,
            new RefreshConnectionAuthenticationOptions
            {
                AuthenticationExpiresOn = expiresAt,
                Claims = BuildClaims(userId, role),
                TokenLifetime = serviceTokenLifetime,
            },
            httpContext.RequestAborted);

        return Results.Json(new
        {
            accessToken = result.AccessToken,
            tokenLifetimeSeconds = result.TokenLifetimeSeconds,
        });
    }
    catch (AzureSignalRException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound(new { error = "connection_not_found" });
    }
    catch (AzureSignalRException ex) when (ex.Message.Contains("different user", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(
            new { error = "permission_change_rejected" },
            statusCode: StatusCodes.Status403Forbidden);
    }
    catch (ArgumentOutOfRangeException)
    {
        return Results.BadRequest(new { error = "invalid_expiration" });
    }
    catch (AzureSignalRException)
    {
        return Results.Json(
            new { error = "internal_server_error" },
            statusCode: StatusCodes.Status500InternalServerError);
    }
}).RequireAuthorization();

app.Run();

static string? GetUserId(ClaimsPrincipal user) =>
    user.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

static string? GetRole(ClaimsPrincipal user) =>
    user.FindFirstValue(ClaimTypes.Role)
    ?? user.FindFirstValue("role");

static List<Claim> BuildClaims(string userId, string? role)
{
    var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
    if (!string.IsNullOrEmpty(role))
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }
    return claims;
}