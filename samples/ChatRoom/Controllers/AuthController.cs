// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.SignalR;
    using Microsoft.Extensions.Configuration;

    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly EndpointProvider _endpointProvider;
        private readonly TokenProvider _tokenProvider;

        public AuthController(IConfiguration config)
        {
            var connStr = config[Constants.AzureSignalRConnectionStringKey];
            _endpointProvider = CloudSignalR.CreateEndpointProviderFromConnectionString(connStr);
            _tokenProvider = CloudSignalR.CreateTokenProviderFromConnectionString(connStr);
        }

        [HttpGet("{hubName}")]
        public IActionResult GenerateJwtBearer(string hubName, [FromQuery] string uid)
        {
            var serviceUrl = _endpointProvider.GetClientEndpoint(hubName);
            var accessToken =_tokenProvider.GenerateClientAccessToken(hubName, new[]
            {
                new Claim(ClaimTypes.NameIdentifier, uid)
            });
            return new OkObjectResult(new
            {
                ServiceUrl = serviceUrl,
                AccessToken = accessToken
            });
        }
    }
}
