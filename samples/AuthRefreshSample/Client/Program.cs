// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AuthRefreshSample;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;

// Usage: dotnet run [hubUrl] [userId] [role]
var hubUrl = args.Length > 0 ? args[0] : "http://localhost:5000/chat";
var userId = args.Length > 1 ? args[1] : "alice";
var role = args.Length > 2 ? args[2] : "user";

// Short lifetime so a refresh happens quickly (the .NET client re-mints the app token before it expires).
var tokenLifetime = TimeSpan.FromMinutes(2);

// Mint a fresh app token on demand.
string MintAppToken()
{
    var now = DateTimeOffset.UtcNow;
    var credentials = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DemoAuth.SigningKey)),
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: DemoAuth.Issuer,
        audience: DemoAuth.Audience,
        claims:
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("name", userId),
            new Claim(ClaimTypes.Role, role),
        ],
        notBefore: now.UtcDateTime,
        expires: now.Add(tokenLifetime).UtcDateTime,
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult<string?>(MintAppToken());
    })
    .WithAuthenticationRefresh(refresh =>
    {
        refresh.EnableAutoRefresh = true; // schedule refresh off tokenLifetimeSeconds
        refresh.RefreshBeforeExpiration = TimeSpan.FromSeconds(30);
        refresh.OnAuthenticationRefreshed = ctx =>
        {
            Console.WriteLine($"[refresh] succeeded; next lifetime = {ctx.NewTokenLifetime}");
            return Task.CompletedTask;
        };
        refresh.OnAuthenticationRefreshFailed = ctx =>
        {
            Console.WriteLine($"[refresh] FAILED: {ctx.Exception?.Message}");
            return Task.CompletedTask;
        };
    })
    .WithAutomaticReconnect()
    .Build();

Console.WriteLine($"Connecting to {hubUrl} as '{userId}' (role '{role}')...");
await connection.StartAsync();
Console.WriteLine("Connected. Type /refresh to refresh authentication (empty line to quit).");

while (true)
{
    var line = Console.ReadLine();
    if (string.IsNullOrEmpty(line))
    {
        break;
    }

    if (string.Equals(line, "/refresh", StringComparison.OrdinalIgnoreCase))
    {
        var newTokenLifetime = await connection.RefreshAuthenticationAsync();
        Console.WriteLine($"[refresh] manually completed; next lifetime = {newTokenLifetime}");
        continue;
    }

    Console.WriteLine("Unknown command. Type /refresh or press Enter to quit.");
}

await connection.DisposeAsync();
