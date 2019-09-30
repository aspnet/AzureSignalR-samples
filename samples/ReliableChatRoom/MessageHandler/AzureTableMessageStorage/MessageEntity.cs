// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public class MessageEntity : TableEntity
    {
        public string SenderName { get; set; }

        public DateTime SendTime { get; set; }

        public string MessageContent { get; set; }

        public string MessageStatus { get; set; }

        public MessageEntity() { }

        public MessageEntity(string pkey, string rkey, Message message)
        {
            PartitionKey = pkey;
            RowKey = rkey;
            SenderName = message.SenderName;
            SendTime = message.SendTime;
            MessageContent = message.MessageContent;
            MessageStatus = message.MessageStatus;
        }

        public void UpdateMessageStatus(string messageStatus)
        {
            MessageStatus = messageStatus;
        }

        public Message ToMessage()
        {
            return new Message(SenderName, SendTime, MessageContent, MessageStatus)
            {
                SequenceId = RowKey
            };
        }
    }
}
