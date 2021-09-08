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
        private readonly ServiceHubContext _messageHubContext;
        private readonly ServiceHubContext _chatHubContext;

        public NegotiateController(IHubContextStore store)
        {
            _messageHubContext = store.MessageHubContext;
            _chatHubContext = store.ChatHubContext;
        }

        [HttpPost("message/negotiate")]
        public Task<ActionResult> MessageHubNegotiate(string user)
        {
            return NegotiateBase(user, _messageHubContext);
        }

        //This API is not used. Just demonstrate a way to have multiple hubs.
        [HttpPost("chat/negotiate")]
        public Task<ActionResult> ChatHubNegotiate(string user)
        {
            return NegotiateBase(user, _chatHubContext);
        }

        private async Task<ActionResult> NegotiateBase(string user, ServiceHubContext serviceHubContext)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var negotiateResponse = await serviceHubContext.NegotiateAsync(new() { UserId = user });

            return new JsonResult(new Dictionary<string, string>()
            {
                { "url", negotiateResponse.Url },
                { "accessToken", negotiateResponse.AccessToken }
            });
        }
    }
}