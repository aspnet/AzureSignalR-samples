// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeSignIn
{
    class AzureSignalR
    {
        class PayloadMessage
        {
            public string Target { get; set; }

            public object[] Arguments { get; set; }
        }

        private readonly string endpoint;

        private readonly string accessKey;

        private readonly HttpClient httpClient = new HttpClient();

        private readonly JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();

        private static void ParseConnectionString(string connectionString, out string endpoint, out string accessKey)
        {
            var dict = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split(new[] { '=' }, 2)).ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());
            if (!dict.TryGetValue("endpoint", out endpoint)) throw new ArgumentException("Invalid connection string, missing endpoint.");
            if (!dict.TryGetValue("accesskey", out accessKey)) throw new ArgumentException("Invalid connection string, missing access key.");
        }

        private string GenerateJwtBearer(string issuer, string audience, ClaimsIdentity subject, DateTime? expires, string signingKey)
        {
            SigningCredentials credentials = null;
            if (!string.IsNullOrEmpty(signingKey))
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            }

            var token = jwtTokenHandler.CreateJwtSecurityToken(
                issuer: issuer,
                audience: audience,
                subject: subject,
                expires: expires,
                signingCredentials: credentials);
            return jwtTokenHandler.WriteToken(token);
        }

        private Task<HttpResponseMessage> PostJsonAsync(string url, object body, string bearer)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptCharset.Clear();
            request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));

            var content = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return httpClient.SendAsync(request);
        }

        public AzureSignalR(string connectionString)
        {
            ParseConnectionString(connectionString, out endpoint, out accessKey);
        }

        public async Task SendAsync(string hubName, string methodName, params object[] args)
        {
            var payload = new PayloadMessage()
            {
                Target = methodName,
                Arguments = args
            };
            var url = $"{endpoint}/api/v1/hubs/{hubName}";
            var bearer = GenerateJwtBearer(null, url, null, DateTime.UtcNow.AddMinutes(30), accessKey);
            await PostJsonAsync(url, payload, bearer);
        }

        public string GetClientHubUrl(string hubName)
        {
            return $"{endpoint}/client/?hub={hubName}";
        }

        public string GenerateAccessToken(string hubName)
        {
            return GenerateJwtBearer(null, GetClientHubUrl(hubName), null, DateTime.UtcNow.AddMinutes(30), accessKey);
        }
    }
}
