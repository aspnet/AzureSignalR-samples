using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RealtimeCodeEditor.Models;
using RealtimeCodeEditor.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RealtimeCodeEditor.Hubs
{
    public class CodeEditorHub : Hub
    {
        private readonly ILogger<CodeEditorHub> _logger;
        private readonly SessionHandler _sessionHandler;

        public CodeEditorHub(ILogger<CodeEditorHub> logger, SessionHandler sessionHandler)
        {
            _logger = logger;
            _sessionHandler = sessionHandler;
        }

        private bool CheckSessionState(string sessionCode, string user)
        {
            if (!_sessionHandler.IsLegalUser(sessionCode, user)) {
                Clients.Client(Context.ConnectionId).SendAsync("expireSession");
                return false;
            }

            return true;
        }

        public async Task OnEnterSession(string sessionCode, string user)
        {
            _logger.LogInformation(Context.User.Identity.Name);
            if (CheckSessionState(sessionCode, user)) {
                _logger.LogInformation("OnEnterSession code: {0}, user: {1}", sessionCode, user);

                await Groups.AddToGroupAsync(Context.ConnectionId, sessionCode);

                await Clients.Client(Context.ConnectionId).SendAsync("enableEditor");
            }
        }

        public async Task OnCodeEditorStateChanged(string sessionCode, string user, string content)
        {
            if (CheckSessionState(sessionCode, user))
            {
                _logger.LogInformation("OnCodeEditorStateChanged");
                _sessionHandler.UpdateSessionState(sessionCode, content);

                await Clients.GroupExcept(sessionCode, Context.ConnectionId).SendAsync("updateCodeEditor", content);
            }
        }

        public async Task OnCodeEditorLocked(string sessionCode, string user)
        {
            if (CheckSessionState(sessionCode, user))
            {
                if (!_sessionHandler.IsLegalCreator(sessionCode, user))
                {
                    return;
                }

                _logger.LogInformation("OnCodeEditorLocked");
                _sessionHandler.LockSession(sessionCode);

                await Clients.Group(sessionCode).SendAsync("lockCodeEditor");
            }
        }

        public async Task OnCodeEditorUnlocked(string sessionCode, string user)
        {
            if (CheckSessionState(sessionCode, user))
            {
                if (!_sessionHandler.IsLegalCreator(sessionCode, user))
                {
                    return;
                }

                _logger.LogInformation("OnCodeEditorUnlocked");
                _sessionHandler.UnlockSession(sessionCode);

                await Clients.Group(sessionCode).SendAsync("unlockCodeEditor");
            }
        }
    }
}
