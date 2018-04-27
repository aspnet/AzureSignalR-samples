// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class Startup
    {
        private const string GitHubClientId = "GitHubClientId";
        private const string GitHubClientSecret = "GitHubClientSecret";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration[GitHubClientId];
                    options.ClientSecret = Configuration[GitHubClientSecret];
                    options.Scope.Add("user:email");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = GetUserCompanyInfoAsync
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Microsoft_Only", policy => policy.RequireClaim("Company", "Microsoft"));
            });

            services.AddMvc();

            services.AddSignalR()
                    .AddAzureSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseMvc();
            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            {
                routes.MapHub<Chat>("/chat");
            });
        }

        private static async Task GetUserCompanyInfoAsync(OAuthCreatingTicketContext context)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);

            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (user.ContainsKey("company"))
            {
                var company = user["company"].ToString();
                var companyIdentity = new ClaimsIdentity(new[]
                {
                    new Claim("Company", company)
                });
                context.Principal.AddIdentity(companyIdentity);
            }
        }
    }
}
