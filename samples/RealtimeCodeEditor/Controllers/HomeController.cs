using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealtimeCodeEditor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealtimeCodeEditor.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SessionHandler _sessionHandler;

        public HomeController(ILogger<HomeController> logger, SessionHandler sessionHandler)
        {
            _logger = logger;
            _sessionHandler = sessionHandler;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Session(string code)
        {
            string requestSourceUser = User.Claims.Where(claim => claim.Type == "preferred_username").First().Value;
            if (_sessionHandler.IsLegalUser(code, requestSourceUser))
            {
                _sessionHandler.JoinSession(code, requestSourceUser);
                ViewBag.SessionModel = _sessionHandler.GenerateSessionModel(code, requestSourceUser);
                return View("~/Views/Home/CodeEditor.cshtml");
            }
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult StartNewSession(string user)
        {
            _logger.LogInformation("StartNewSession user: {0}", user);
            string sessionCode = _sessionHandler.CreateSession(user);
            return Redirect("/Home/Session?code=" + sessionCode);
        }

        [HttpPost]
        public IActionResult EnterSession(string user, string sessionCode)
        {
            _logger.LogInformation("EnterSession user: {0}, code: {1}", user, sessionCode);
            return Redirect("/Home/Session?code=" + sessionCode);
        }
    }
}
