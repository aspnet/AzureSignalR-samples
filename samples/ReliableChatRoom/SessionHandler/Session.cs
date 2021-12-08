// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class Session
    {
        public string SessionId { get; }

        public Session(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
