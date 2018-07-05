using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.Azure.SignalR.Sample.ConsoleSample;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ConsoleSample
{
    public class ServerHandler
    {
        private static readonly HttpClient Client = new HttpClient();
        private static string _serverName;
        private readonly ServiceUtils ServiceUtils;
        private string _hubName;
        private string _endpoint;

        private readonly PayloadMessage DefaultPayloadMessage;

        public ServerHandler(string connectionString, string hubName)
        {
            _serverName = GenerateServerName();

            ServiceUtils = new ServiceUtils(connectionString);
            _hubName = hubName;
            _endpoint = ServiceUtils.Endpoint;

            DefaultPayloadMessage = new PayloadMessage
            {
                Target = "SendMessage",
                Arguments = new[]
                {
                    _serverName,
                    "Sent from rest api call",
                }
            };
        }

        public async Task Start()
        {
            ShowHelp();
            while (true)
            {
                var argLine = Console.ReadLine();
                if (argLine == null)
                {
                    continue;
                }
                var args = argLine.Split(' ');

                if (args.Length == 1 && args[0].Equals("broadcast"))
                {
                    await BroadCast(_hubName);
                }
                else if (args.Length == 3 && args[0].Equals("send"))
                {
                    switch (args[1])
                    {
                        case "user":
                            await SendToUser(_hubName, args[2]);
                            break;
                        case "users":
                            await SendToUsers(_hubName, args[2]);
                            break;
                    }
                }
            }
        }

        public async Task SendToUser(string hubName, string userId)
        {
            try
            {
                var url = GetSendToUserUrl(hubName, userId);
                var request = BuildRequest(url);

                var response = await Client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    throw new Exception("sent error");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task SendToUsers(string hubName, string userList)
        {
            var url = GetSendToUsersUrl(hubName, userList);
            var request = BuildRequest(url);
            var response = await Client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Sent error: {response.StatusCode}");
            }
        }

        public async Task BroadCast(string hubName)
        {
            var url = GetBroadcastUrl(hubName);
            var request = BuildRequest(url);
            var response = await Client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                Console.WriteLine($"Sent error: {response.StatusCode}");
            }
        }

        private Uri GetUrl(string baseUrl)
        {
            return new UriBuilder(baseUrl).Uri;
        }

        private string GetSendToUserUrl(string hubName, string userId)
        {
            return $"{GetBaseUrl(hubName)}/user/{userId}";
        }

        private string GetSendToUsersUrl(string hubName, string userList)
        {
            return $"{GetBaseUrl(hubName)}/users/{userList}";
        }

        private string GetBroadcastUrl(string hubName)
        {
            return $"{GetBaseUrl(hubName)}";
        }

        private string GetBaseUrl(string hubName)
        {
            return $"{_endpoint}:5002/api/v1-preview/hub/{hubName.ToLower()}";
        }

        private string GenerateServerName()
        {
            // Use the machine name for convenient diagnostics, but add a guid to make it unique.
            // Example: MyServerName_02db60e5fab243b890a847fa5c4dcb29
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private HttpRequestMessage BuildRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", ServiceUtils.GenerateServerAccessToken(url, _serverName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(DefaultPayloadMessage), Encoding.UTF8, "application/json");

            return request;
        }

        private void ShowHelp()
        {
            Console.WriteLine("*********Usage*********\n" +
                              "send user <User Id>\n" +
                              "send users <User Id List>\n" + 
                              "broadcase\n" +
                              "***********************");
        }
    }

    public class PayloadMessage
    {
        public string Target { get; set; }

        public object[] Arguments { get; set; }
    }
}