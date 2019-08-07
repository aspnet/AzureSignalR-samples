// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = JwtController.Issuer,
                        ValidAudience = JwtController.Audience,
                        IssuerSigningKey = JwtController.SigningCreds.Key
                    };
                });

            services.AddMvc();
            services.AddSignalR()
                    .AddAzureSignalR(options =>
            {
                options.ClaimsProvider = context => new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.Request.Query["username"])
                };
            });
            services.AddSingleton<IMessageHandler, StaticMessageStorage>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvc();
            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<ChatRoomWithAck>("/chat");
            });
        }
    }
}
