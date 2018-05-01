using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.SignalR;

namespace RealtimeSignIn
{
    class AuthInfo
    {
        public string ServiceUrl { get; set; }
        public string AccessToken { get; set; }
    }

    static class SignInHub
    {
        private const string HubName = "signIn";

        private static readonly string connectionString = Environment.GetEnvironmentVariable("AzureSignalRConnectionString");

        private static readonly EndpointProvider endpointProvider = CloudSignalR.CreateEndpointProviderFromConnectionString(connectionString);

        private static readonly TokenProvider tokenProvider = CloudSignalR.CreateTokenProviderFromConnectionString(connectionString);

        private static readonly HubProxy hubProxy = CloudSignalR.CreateHubProxyFromConnectionString(connectionString, HubName);

        public static async Task UpdateSignInStats(SignInStats stats)
        {
            await hubProxy.Clients.All.SendAsync("updateSignInStats", new object[] { stats });
        }

        public static AuthInfo GetAuthInfo()
        {
            return new AuthInfo()
            {
                ServiceUrl = endpointProvider.GetClientEndpoint(HubName),
                AccessToken = tokenProvider.GenerateClientAccessToken(HubName)
            };
        }
    }
}
