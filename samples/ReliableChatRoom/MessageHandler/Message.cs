// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class Message : IComparable<Message>
    {
        public string SenderName { get; }

        public DateTime SendTime { get; }

        public string SequenceId { get; set; }

        public string MessageContent { get; set; }

        public string MessageStatus { get; set; }

        public Message(string senderName, DateTime sendTime, string messageContent, string messageStatus)
        {
            SenderName = senderName;
            SendTime = sendTime;
            MessageContent = messageContent;
            MessageStatus = messageStatus;
        }

        public int CompareTo(Message message)
        {
            return SequenceId.CompareTo(message.SequenceId);
        }
    }
}
