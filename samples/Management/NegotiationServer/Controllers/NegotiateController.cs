// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;

namespace NegotiationServer.Controllers
{
    [ApiController]
    public class NegotiateController : ControllerBase
    {
        private const string EnableDetailedErrors = "EnableDetailedErrors";
        private readonly ServiceHubContext _hubContext;
        private readonly ServiceHubContext<IMessageClient> _stronglyTypedHubContext;
        private readonly bool _enableDetailedErrors;

        public NegotiateController(IHubContextStore store, IConfiguration configuration)
        {
            _hubContext = store.HubContext;
            _stronglyTypedHubContext = store.StronglyTypedHubContext;
            _enableDetailedErrors = configuration.GetValue(EnableDetailedErrors, false);
        }

        [HttpPost("hub/negotiate")]
        public async Task<ActionResult> HubNegotiate(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var negotiateResponse = await _hubContext.NegotiateAsync(new()
            {
                UserId = user,
                EnableDetailedErrors = _enableDetailedErrors
            });

            return new JsonResult(new Dictionary<string, string>()
            {
                { "url", negotiateResponse.Url },
                { "accessToken", negotiateResponse.AccessToken }
            });
        }

        //The negotiation of strongly typed hub has little difference with untyped hub.
        [HttpPost("stronglyTypedHub/negotiate")]
        public async Task<ActionResult> StronglyTypedHubNegotiate(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var negotiateResponse = await _stronglyTypedHubContext.NegotiateAsync(new()
            {
                UserId = user,
                EnableDetailedErrors = _enableDetailedErrors
            });

            return new JsonResult(new Dictionary<string, string>()
            {
                { "url", negotiateResponse.Url },
                { "accessToken", negotiateResponse.AccessToken }
            });
        }
    }
}