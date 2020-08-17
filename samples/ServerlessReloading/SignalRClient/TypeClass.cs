// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
namespace SignalRClient
{
    public class Message
    {
        public string msgType { get; set; }
        // 0 : just messages
        // 1 : ReloadMessage
        // 2 : FinMessage
        // 3 : AckMessage

        public string content { get; set; }
    }

    // Used to notify client that you need to reload connection with new url and token
    public class ReloadMessage
    {
        public string url { get; set; }
        public string token { get; set; }
    }
    
    // Used as Barrier Message
    public class FinMessage
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    // When old service has received the Barrier message sent by the client, it will send this AckMessage to the client
    public class AckMessage
    {
        // This connection is over.
        public string connID { get; set; }
    }
}
