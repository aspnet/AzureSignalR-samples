// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;

namespace NegotiationServer.Controllers
{
    [ApiController]
    public class NegotiateController : ControllerBase
    {
        private readonly ServiceHubContext _hubContext;

        public NegotiateController(SignalRService signalrService)
        {
            _hubContext = signalrService.HubContext;
        }

        [HttpPost("ManagementSampleHub/negotiate")]
        public async Task<ActionResult> Index(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var negotiateResponse = await _hubContext.NegotiateAsync(new() { UserId = user });

            return new JsonResult(new Dictionary<string, string>()
            {
                { "url", negotiateResponse.Url },
                { "accessToken", negotiateResponse.AccessToken }
            });
        }
    }
}