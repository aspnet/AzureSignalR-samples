// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;

namespace NegotiationServer.Controllers
{
    [ApiController]
    public class NegotiateController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NegotiateController(IConfiguration configuration)
        {
            _config = configuration;
        }

        [HttpPost("{hub}/negotiate")]
        public ActionResult Index(string hub, string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var connectionString = _config["Azure:SignalR:ConnectionString"];
            var (endpoint, accessKey, version, port) = ParseConnectionString(connectionString);
            var newEndpoint = hub == "Auth" ? endpoint + ":6787" : endpoint;
            var newConnectionString = CombineConnectionString(newEndpoint, accessKey, version, port);
            var url = GetServiceManager(connectionString).GetClientEndpoint(hub);
            var accessToken = GetServiceManager(newConnectionString).GenerateClientAccessToken(hub, user);
            return new JsonResult(new Dictionary<string, string>()
            {
                { "url",  url},
                { "accessToken",  accessToken}
            });
        }

        private IServiceManager GetServiceManager(string connectionString)
        {
            return new ServiceManagerBuilder()
                .WithOptions(o => o.ConnectionString = connectionString)
                .Build();
        }

        private (string endpoint, string accessToken, string version, string port) ParseConnectionString(string connectionString)
        {
            var parts = connectionString.Split(";");
            var endpoint = "";
            var accessToken = "";
            var version = "";
            var port = "";
            
            foreach (var part in parts)
            {
                if (part.StartsWith("Endpoint="))
                {
                    endpoint = part.Substring("Endpoint=".Length);
                }

                if (part.StartsWith("AccessKey="))
                {
                    accessToken = part.Substring("AccessKey=".Length);
                }

                if (part.StartsWith("Version="))
                {
                    version = part.Substring("Version=".Length);
                }

                if (part.StartsWith("Port="))
                {
                    port = part.Substring("Port=".Length);
                }
            }

            return (endpoint, accessToken, version, port);
        }

        private string CombineConnectionString(string endpoint, string accessKey, string version, string port)
        {
            var connectionString = "";
            if (!string.IsNullOrEmpty(endpoint))
            {
                connectionString += $"Endpoint={endpoint};";
            }

            if (!string.IsNullOrEmpty(accessKey))
            {
                connectionString += $"AccessKey={accessKey};";
            }

            if (!string.IsNullOrEmpty(version))
            {
                connectionString += $"Version={version};";
            }

            if (!string.IsNullOrEmpty(port))
            {
                connectionString += $"Port={port};";
            }

            return connectionString;
        }
    }
}