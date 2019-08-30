// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{
    public class FlightMapSampleHub : Hub
    {
        private IFlightControl control;

        public FlightMapSampleHub(IFlightControl ctrl)
        {
            control = ctrl;
        }

        public override Task OnConnectedAsync()
        {
            return Clients.All.SendAsync("updateVisitors", control.VisitorJoin());
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Clients.All.SendAsync("updateVisitors", control.VisitorLeave());
        }
    }
}
