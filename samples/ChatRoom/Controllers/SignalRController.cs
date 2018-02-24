// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.SignalR;
    using Microsoft.Extensions.Configuration;

    [Route("api/signalr")]
    public class SignalRController : Controller
    {
        private readonly SignalRService _signalr;

        public SignalRController(IConfiguration config)
        {
            _signalr = SignalRService.CreateFromConnectionString(config[Constants.AzureSignalRConnectionStringKey]);
        }

        [HttpGet("{hubName}")]
        public IActionResult GenerateJwtBearer(string hubName, [FromQuery] string uid)
        {
            var serviceUrl = $"{_signalr.GetClientUrl(hubName)}&uid={uid}";
            var accessToken = _signalr.GenerateClientToken(hubName, new[]
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
