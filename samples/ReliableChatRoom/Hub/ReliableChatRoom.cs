// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class ReliableChatRoom : Hub
    {
        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public string SendUserMessage(string messageId, string receiver, string messageContent)
        {
            var sender = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Clients.User(receiver).SendAsync("displayUserMessage", messageId, sender, messageContent);

            //  TODO: Create and add the new message to the storage

            return "Sent";
        }

        public string SendUserAck(string messageId, string sender, string messageStatus)
        {
            //  TODO: Update the messageStatus in the storage

            Clients.User(sender).SendAsync("displayAckMessage", messageId, messageStatus);

            return "Sent";
        }
    }
}
