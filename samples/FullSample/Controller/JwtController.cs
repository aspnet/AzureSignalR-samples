using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    [Route("jwt")]
    public class JwtController : Controller
    {
        [HttpGet]
        public IActionResult Login([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }
        }
    }
}
