// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    [Route("api/auth")]
    public class AuthController : Controller
    {
        public const string GitHubClientIdKey = "GitHubClientId";
        public const string GitHubClientSecretKey = "GitHubClientSecret";

        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AuthController(IConfiguration config)
        {
            _clientId = config[GitHubClientIdKey];
            _clientSecret = config[GitHubClientSecretKey];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHubChat");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Redirect($"https://github.com/login/oauth/authorize?scope=user:email&client_id={_clientId}");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var githubToken = await GetTokenAsync(code);
            var user = await GetUserAsync(githubToken);
            var claimsPrincipal = GetClaimsPrincipal(user);

            Response.Cookies.Append("githubchat_username", user.Name);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return Redirect("/");
        }

        private async Task<string> GetTokenAsync(string code)
        {
            var body = JsonConvert.SerializeObject(new Dictionary<string, string> {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "code", code },
                { "accept", "json" }
            });
            var response = await _httpClient.PostAsync("https://github.com/login/oauth/access_token", new StringContent(body, Encoding.UTF8, "application/json"));
            var tokenString = await response.Content.ReadAsStringAsync();
            var tokenObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenString);
            return tokenObject["access_token"];
        }

        private async Task<UserInfo> GetUserAsync(string token)
        {
            var userString = await _httpClient.GetStringAsync($"https://api.github.com/user?access_token={token}");
            var userObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(userString);
            return new UserInfo
            {
                Name = userObject["login"],
                Company = userObject["company"]
            };
        }

        private static ClaimsPrincipal GetClaimsPrincipal(UserInfo user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Name),
                new Claim("Company", user.Company ?? string.Empty)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        private class UserInfo
        {
            public string Name { get; set; }

            public string Company { get; set; }
        }
    }
}
