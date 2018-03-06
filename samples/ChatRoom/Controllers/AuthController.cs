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
        private readonly SignalRService _service;

        public AuthController(IConfiguration config)
        {
            _service = SignalRService.CreateFromConnectionString(config[Constants.AzureSignalRConnectionStringKey]);
        }

        [HttpGet("{hubName}")]
        public IActionResult GenerateJwtBearer(string hubName, [FromQuery] string uid)
        {
            var serviceUrl = $"{_service.GetClientUrl(hubName)}&uid={uid}";
            var accessToken = _service.GenerateClientToken(hubName, new[]
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
