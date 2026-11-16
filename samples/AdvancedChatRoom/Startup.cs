// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Microsoft.Azure.SignalR.Samples.AdvancedChatRoom
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(option =>
            {
                option.AddPolicy("ClaimBasedAuth", policy =>
                    {
                        policy.RequireClaim(ClaimTypes.NameIdentifier);
                    });
                option.AddPolicy("PolicyBasedAuth", policy => policy.Requirements.Add(new PolicyBasedAuthRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, PolicyBasedAuthHandler>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = JwtController.Issuer,
                        ValidAudience = JwtController.Audience,
                        IssuerSigningKey = JwtController.SigningCreds.Key
                    };
                    option.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.SecurityToken is JsonWebToken jwt)
                            {
                                context.Properties.ExpiresUtc = new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero);
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddControllers();
            services.AddSignalR()
                .AddAzureSignalR(options =>
            {
                options.ClaimsProvider = context => new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseRouting();
            app.UseFileServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatJwtSampleHub>("/chatjwt", options =>
                {
                    options.EnableAuthenticationRefresh = true;
                    options.CloseOnAuthenticationExpiration = true;
                });
                endpoints.MapHub<ChatCookieSampleHub>("/chatcookie");
            });
        }
    }
}
