// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{

    [Route("/animation")]
    public class AnimationController : Controller
    {
        private IFlightControl control;

        private string adminKey;

        public AnimationController(IFlightControl ctrl, IConfiguration configuration)
        {
            adminKey = configuration["AdminKey"];
            control = ctrl;
        }

        [HttpGet("start")]
        public IActionResult Start(string key)
        {
            if (string.IsNullOrEmpty(adminKey) || key != adminKey) return new UnauthorizedResult();
            control.Start();
            return new OkObjectResult(new
            {
                Message = "Started"
            });
        }

        [HttpGet("stop")]
        public IActionResult Stop(string key)
        {
            if (string.IsNullOrEmpty(adminKey) || key != adminKey) return new UnauthorizedResult();
            control.Stop();
            return new OkObjectResult(new
            {
                Message = "Stopped"
            });
        }

        [HttpGet("restart")]
        public IActionResult Restart(string key)
        {
            if (string.IsNullOrEmpty(adminKey) || key != adminKey) return new UnauthorizedResult();
            control.Restart();
            return new OkObjectResult(new
            {
                Message = "Restarted"
            });
        }

        [HttpGet("setSpeed")]
        public IActionResult SetSpeed(int speed, string key)
        {
            if (string.IsNullOrEmpty(adminKey) || key != adminKey) return new UnauthorizedResult();
            if (speed < 1 || speed > 10) return new BadRequestObjectResult("Speed must between 1 and 10.");
            control.SetSpeed(speed);
            return new OkObjectResult(new
            {
                Message = "Speed set"
            });
        }
    }
}