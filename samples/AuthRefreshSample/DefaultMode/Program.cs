// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System.Text;

using AuthRefreshSample;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

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
            // Auth refresh advertises tokenLifetimeSeconds only when the auth ticket carries an ExpiresUtc. 
            // JwtBearer does not set it from the token by default, so surface the token's exp.
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
builder.Services.AddSignalR().AddAzureSignalR(); // Azure:SignalR:ConnectionString

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat", options =>
{

    options.CloseOnAuthenticationExpiration = true;
    options.EnableAuthenticationRefresh = true;
    // Optional accept/reject gate, run before Azure SignalR mutates anything.
    options.OnAuthenticationRefresh = context =>
        ValueTask.FromResult(!context.NewUser.IsInRole("blocked"));
}).RequireAuthorization();

app.Run();
