using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.Azure.SignalR.Samples.ChatRoom
{
    [Route("cookie")]
    public class CookieController : Controller
    {
        [HttpGet("login")]
        public async Task Login()
        {
            string username = HttpContext.Request.Query["username"];
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, HttpContext.Request.Query["role"])
            };

            if (username.StartsWith("cookie"))
            {
                claims.Add(new Claim("AuthorizedUser", username));
            }
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //User.AddIdentity(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            //await HttpContext.SignInAsync(User);
        }

    }
}