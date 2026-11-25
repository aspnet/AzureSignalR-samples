// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Common;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NegotiationServer.Controllers
{
    [ApiController]
    public class NegotiateController : ControllerBase
    {
        private const string EnableDetailedErrors = "EnableDetailedErrors";
        private readonly ServiceHubContext _messageHubContext;
        private readonly ServiceHubContext _chatHubContext;
        private readonly bool _enableDetailedErrors;
        private readonly ILogger<NegotiateController> _logger;

        public NegotiateController(IHubContextStore store, IConfiguration configuration, ILogger<NegotiateController> logger)
        {
            _messageHubContext = store.MessageHubContext;
            _chatHubContext = store.ChatHubContext;
            _enableDetailedErrors = configuration.GetValue(EnableDetailedErrors, false);
            _logger = logger;
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

        [HttpPost("message/refresh")]
        public Task<ActionResult> MessageHubRefresh()
        {
            return RefreshBase(_messageHubContext);
        }

        [HttpPost("chat/refresh")]
        public Task<ActionResult> ChatHubRefresh()
        {
            return RefreshBase(_chatHubContext);
        }

        private async Task<ActionResult> NegotiateBase(string user, ServiceHubContext serviceHubContext)
        {
            if (string.IsNullOrEmpty(user))
            {
                return BadRequest("User ID is null or empty.");
            }

            var negotiateResponse = await serviceHubContext.NegotiateWithTokenLifetimeAsync(new()
            {
                UserId = user,
                EnableDetailedErrors = _enableDetailedErrors
            }, HttpContext.RequestAborted);

            return new JsonResult(new
            {
                url = negotiateResponse.Url,
                accessToken = negotiateResponse.AccessToken,
                tokenLifetimeSeconds = negotiateResponse.TokenLifetimeSeconds
            });
        }

        private async Task<ActionResult> RefreshBase(ServiceHubContext serviceHubContext)
        {
            var connectionToken = Request.Headers["X-SignalR-Connection-Token"].ToString();
            if (string.IsNullOrEmpty(connectionToken))
            {
                return BadRequest(new { error = "missing_connection_token" });
            }

            try
            {
                var result = await serviceHubContext.RefreshConnectionAuthenticationAsync(
                    connectionToken,
                    new RefreshConnectionAuthenticationOptions(),
                    HttpContext.RequestAborted);

                return new JsonResult(new
                {
                    accessToken = result.AccessToken,
                    tokenLifetimeSeconds = result.TokenLifetimeSeconds
                });
            }
            catch (AzureSignalRException ex)
            {
                _logger.LogError(ex, "Failed to refresh connection authentication.");
                return StatusCode(500, new { error = "internal_server_error" });
            }
        }
    }
}