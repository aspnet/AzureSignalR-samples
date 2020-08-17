// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    public class MessagePublisher
    {
        private const string Target = "Target";
        private const string HubName = "ManagementSampleHub";
        private readonly string _connectionString;
        private readonly ServiceTransportType _serviceTransportType;
        private IServiceHubContext _hubContext;
        // reload connection string 
        private readonly string connectionString = "Endpoint=http://localhost;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Port=8081;Version=1.0;";


        public MessagePublisher(string connectionString, ServiceTransportType serviceTransportType)
        {
            _connectionString = connectionString;
            _serviceTransportType = serviceTransportType;
        }

        public async Task InitAsync()
        {
            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = _connectionString;
                option.ServiceTransportType = _serviceTransportType;
            }).Build();

            _hubContext = await serviceManager.CreateHubContextAsync(HubName, new LoggerFactory());
        }

        public Task ManageUserGroup(string command, string userId, string groupName)
        {
            switch (command)
            {
                case "add":
                    return _hubContext.UserGroups.AddToGroupAsync(userId, groupName);
                case "remove":
                    return _hubContext.UserGroups.RemoveFromGroupAsync(userId, groupName);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        public Task SendMessages(string command, string receiver, string message)
        {
            var jMsg = new
            {
                msgType = "0",
                content = message
            };
            var uMsg = JsonConvert.SerializeObject(jMsg);
            switch (command)
            {
                case "broadcast":
                    return _hubContext.Clients.All.SendAsync(Target, uMsg);
                case "user":
                    var userId = receiver;
                    return _hubContext.Clients.User(userId).SendAsync(Target, uMsg);
                case "users":
                    var userIds = receiver.Split(',');
                    return _hubContext.Clients.Users(userIds).SendAsync(Target, uMsg);
                case "group":
                    var groupName = receiver;
                    return _hubContext.Clients.Group(groupName).SendAsync(Target, uMsg);
                case "groups":
                    var groupNames = receiver.Split(',');
                    return _hubContext.Clients.Groups(groupNames).SendAsync(Target, uMsg);
                case "reload":
                    HttpClient restClient = new HttpClient();
                    var msg = new
                    {
                        Target = Target,
                        Arguments = new string[] { connectionString }
                    };

                    // generate token
                    string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH";
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    {
                        KeyId = key.GetHashCode().ToString()
                    };
                    SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
                    var token = JwtTokenHandler.CreateJwtSecurityToken(
                        issuer: null,
                        audience: "http://localhost/api/v1/reload",
                        notBefore: DateTime.Now,
                        expires: DateTime.Now.AddHours(1),
                        issuedAt: DateTime.Now,
                        signingCredentials: credentials) ;
                    string tk = JwtTokenHandler.WriteToken(token);

                    restClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tk);
                    restClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string json = JsonConvert.SerializeObject(msg);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    // Call reload rest api on the old service to start reloading connection
                    string url = "http://localhost:8080/api/v1/reload";

                    return restClient.PostAsync(url, data);
                default:
                    Console.WriteLine($"Can't recognize command {command}");
                    return Task.CompletedTask;
            }
        }

        public Task DisposeAsync() => _hubContext?.DisposeAsync();
    }
}