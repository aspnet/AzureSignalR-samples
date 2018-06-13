using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class FullAuthorizedHandler : AuthorizationHandler<FullAuthorizedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            FullAuthorizedRequirement requirement)
        {
            if (context.User.IsInRole("Admin")
                && context.User.HasClaim(c => 
                    c.Type == "AuthorizedUser" && c.Value.StartsWith("cookie")))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
