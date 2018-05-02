// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{

    [Route("/animation")]
    public class AnimationController : Controller {

        IFlightControl control;

        public AnimationController(IFlightControl ctrl) {
            control = ctrl;
        }

        [HttpGet("start")]
        public string Start() {
            control.Start();
            return "Started to animate.";
        }

        [HttpGet("stop")]
        public string Stop() {
            control.Stop();
            return "Stopped animation.";
        }

        [HttpGet("restart")]
        public string restart() {
            control.Restart();
            return "Restarted animation.";
        }
    }
}