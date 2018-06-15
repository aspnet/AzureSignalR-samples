// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Azure.SignalR.Samples.AdvancedChatRoom
{
    public class PolicyBasedAuthRequirement : IAuthorizationRequirement
    {
        public PolicyBasedAuthRequirement()
        {
        }
    }
}
