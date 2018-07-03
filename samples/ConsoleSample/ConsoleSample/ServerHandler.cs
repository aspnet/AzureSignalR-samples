using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ConsoleSample
{
    public class ServerHandler
    {
        private const string Endpoint = "https://testrest.service.signalr.net";
        private const string HubName = "ChatCookie";
        private const string URL = "";
        private const string AccessKey = "G4EHMq0ALOocnGp9G7OIECVe4Qkumcv69buSDgOodQg=";
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
        private static readonly HttpClient client = new HttpClient();

        public ServerHandler()
        {
            
        }

        public async void SendToUser(string userId)
        {
            string connectionString = Guid.NewGuid().ToString();
            //var httpConnectionOptions = new HttpConnectionOptions
            //{
            //    Url = GetServiceUrl($"{GetRestUrl(5002, HubName)}/user/{userId}", connectionString),
            //    AccessTokenProvider = () => Task.FromResult(GenerateServerAccessToken(HubName, userId)),
            //};

            var request = new HttpRequestMessage(HttpMethod.Post, GetServiceUrl($"{GetRestUrl(5002, HubName)}/user/{userId}", connectionString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateServerAccessToken(HubName, userId));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await client.SendAsync(request);

        }

        public void SendToUsers()
        {

        }

        public void BroadCast()
        {

        }

        public void SendToGroup()
        {

        }

        public void SendToGroups()
        {

        }

        private string GenerateServerAccessToken(string hubName, string userId, TimeSpan? lifetime = null)
        {
            IEnumerable<Claim> claims = null;
            if (userId != null)
            {
                claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };
            }

            return GenerateAccessTokenCore(GetEndpoint(5002, "server", HubName), claims, TimeSpan.FromHours(1));
        }

        private string GenerateAccessTokenCore(string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var expire = DateTime.UtcNow.Add(lifetime);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AccessKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: audience,
                subject: new ClaimsIdentity(claims),
                expires: expire,
                signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

        private string GetEndpoint(int port, string path, string hubName)
        {
            return $"{Endpoint}:{port}/{path}/?hub={hubName.ToLower()}";
        }

        private Uri GetServiceUrl(string baseUrl, string connectionId)
        {
            var baseUri = new UriBuilder(baseUrl);
            //var query = "cid=" + connectionId;
            //if (baseUri.Query != null && baseUri.Query.Length > 1)
            //{
            //    baseUri.Query = baseUri.Query.Substring(1) + "&" + query;
            //}
            //else
            //{
            //    baseUri.Query = query;
            //}
            return baseUri.Uri;
        }

        private string GetRestUrl(int port, string hubName)
        {
            return $"{Endpoint}:{port}/api/v1-preview/hub/{hubName.ToLower()}";
        }
    }
}