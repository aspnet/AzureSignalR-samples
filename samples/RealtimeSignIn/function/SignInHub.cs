using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace RealtimeSignIn
{
    class AuthInfo
    {
        public string ServiceUrl { get; set; }
        public string AccessToken { get; set; }
    }

    static class SignInHub
    {
        private static readonly JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();

        private static readonly HttpClient httpClient = new HttpClient();

        private const string ServiceUrl = "https://signalr-chat.servicedev.signalr.net";

        private const string AccessKey = "1Y3YMtHTqYFLQzBd8ni/8jXRzp3giDs+oqJ+IrZSV98=";

        private const string HubName = "signin";

        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureSignalRConnectionString");

        public static TraceWriter logger;

        class PayloadMessage
        {
            public string Target { get; set; }

            public object[] Arguments { get; set; }

            public string[] ExcludedList { get; set; }
        }

        private static Task<HttpResponseMessage> PostJsonAsync(string url, object body, string bearer)
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
            logger.Info(url);
            logger.Info(content);
            return httpClient.SendAsync(request);
        }

        private static async Task<HttpStatusCode> SendAsync(string hub, string method, params object[] args)
        {
            var payload = new PayloadMessage()
            {
                Target = method,
                Arguments = args
            };
            var url = $"{ServiceUrl}:5002/api/v1-preview/hub/{hub}";
            var bearer = GenerateJwtBearer(null, url, null, DateTime.UtcNow.AddMinutes(30), AccessKey);
            var response = await PostJsonAsync(url, payload, bearer);
            return response.StatusCode;
        }

        public static async Task<HttpStatusCode> UpdateSignInStatsAsync(SignInStats stats)
        {
            return await SendAsync(HubName, "updateSignInStats", stats.totalNumber, stats.byOS, stats.byBrowser);
        }

        public static AuthInfo GetAuthInfo()
        {
            var hubUrl = $"{ServiceUrl}:5001/client/?hub={HubName}";
            return new AuthInfo()
            {
                ServiceUrl = hubUrl,
                AccessToken = GenerateJwtBearer(null, hubUrl, null, DateTime.UtcNow.AddMinutes(30), AccessKey)
            };
        }

        private static string GenerateJwtBearer(
            string issuer = null,
            string audience = null,
            ClaimsIdentity subject = null,
            DateTime? expires = null,
            string signingKey = null)
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
    }
}
