using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    public class FullAuthorizedRequirement : IAuthorizationRequirement
    {
        public FullAuthorizedRequirement()
        {

        }
    }
}
