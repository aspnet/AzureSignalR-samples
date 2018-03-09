// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Configuration;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.SignalR;

namespace ChatRoomAspNet.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private static readonly EndpointProvider EndpointProvider = CloudSignalR.CreateEndpointProviderFromConnectionString(ConfigurationManager.AppSettings["AzureSignalRConnectionString"]);
        private static readonly TokenProvider TokenProvider = CloudSignalR.CreateTokenProviderFromConnectionString(ConfigurationManager.AppSettings["AzureSignalRConnectionString"]);

        [HttpGet]
        [Route("{hubName}")]
        public IHttpActionResult GenerateJwtBearer(string hubName, [FromUri] string uid = null)
        {
            var serviceUrl = EndpointProvider.GetClientEndpoint(hubName);
            var accessToken = TokenProvider.GenerateClientAccessToken(hubName, new[]
            {
                new Claim(ClaimTypes.NameIdentifier, uid)
            });
            return Ok(new
            {
                ServiceUrl = serviceUrl,
                AccessToken = accessToken
            });
        }
    }
}
