// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Azure.SignalR.Samples.AdvancedChatRoom
{
    [Route("cookie")]
    public class CookieController : Controller
    {
        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                return BadRequest("Username and role is required.");
            }

            if (!IsExistingUser(username))
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, HttpContext.Request.Query["role"])
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            return Ok();
        }

        private bool IsExistingUser(string username)
        {
            return username.StartsWith("cookie");
        }
    }
}
