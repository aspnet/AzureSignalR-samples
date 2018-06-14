using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class PolicyBasedAuthHandler : AuthorizationHandler<PolicyBasedAuthRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PolicyBasedAuthRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
