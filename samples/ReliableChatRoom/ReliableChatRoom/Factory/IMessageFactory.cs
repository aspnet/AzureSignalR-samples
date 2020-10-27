using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Factory
{
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates a <see cref="MessageTypeEnum.System"/> <see cref="Message"/> according to the given username, action, and sendDate.
        /// </summary>
        /// <param name="username">Username of client</param>
        /// <param name="action">Can be either "left" or "join"</param>
        /// <param name="sendDate">Time when client joined the chat room, i.e. when EnterChatRoom was called</param>
        /// <returns>A <see cref="MessageTypeEnum.System"/> <see cref="Message"/></returns>
        Message CreateSystemMessage(string username, string action, DateTime sendDate);

        /// <summary>
        /// Creates a <see cref="MessageTypeEnum.Broadcast"/> <see cref="Message"/> according to the given username, action, and sendDate.
        /// </summary>
        /// <param name="messageId">A Uuid generated and sent by client. Server-side do not generate messageIds</param>
        /// <param name="sender">Sender of the message</param>
        /// <param name="payload">Content of message. Can be either a text string or rich content represented by a Base64 string</param>
        /// <param name="sendTime">The time when the broadcast message reaches the server are labeled as sendTime</param>
        /// <returns>A <see cref="MessageTypeEnum.Broadcast"/> <see cref="Message"/></returns>
        Message CreateBroadcastMessage(string messageId, string sender, string payload, DateTime sendTime);

        /// <summary>
        /// Creates a <see cref="MessageTypeEnum.Private"/> <see cref="Message"/> according to the given username, action, and sendDate.
        /// </summary>
        /// <param name="messageId">A Uuid generated and sent by client. Server-side do not generate messageIds</param>
        /// <param name="sender">Sender of the message</param>
        /// <param name="receiver">Sender of the message</param>
        /// <param name="payload">Receiver of the message</param>
        /// <param name="sendTime">The time when the broadcast message reaches the server are labeled as sendTime</param>
        /// <returns>A <see cref="MessageTypeEnum.Private"/> <see cref="Message"/></returns>
        Message CreatePrivateMessage(string messageId, string sender, string receiver, string payload, DateTime sendTime);

        /// <summary>
        /// Converts a list of <see cref="Message"/> from a json string.
        /// </summary>
        /// <param name="jsonString">The json string from which a list of messages is created</param>
        /// <returns>The converted list of <see cref="Message"/></returns>
        List<Message> FromListJsonString(string jsonString);

        /// <summary>
        /// Converts an instance of <see cref="Message"/> from a json string.
        /// </summary>
        /// <param name="jsonString">The json string from which an instance of message is created</param>
        /// <returns>The converted instance of <see cref="Message"/></returns>
        Message FromSingleJsonString(string jsonString);

        /// <summary>
        /// Converts a json string from a list of <see cref="Message"/>.
        /// </summary>
        /// <param name="messages">The list of messages from which the json string is created</param>
        /// <returns>The converted json string</returns>
        string ToListJsonString(List<Message> messages);

        /// <summary>
        /// Converts a json string from an instance of <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The instance of message from which the json string is created</param>
        /// <returns>The converted json string</returns>
        string ToSingleJsonString(Message message);
    }
}
