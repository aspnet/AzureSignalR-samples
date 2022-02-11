// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Microsoft.Azure.Functions.Worker
{
    /// <summary>
    /// An action to manage a group.
    /// </summary>
    /// <remarks>
    /// The class will be available by extensions in the next release.
    /// </remarks>
    public sealed class SignalRGroupAction
    {
        /// <summary>
        /// Initializes an instance of a <see cref="SignalRGroupAction"/>.
        /// </summary>
        /// <param name="actionType">The action type.</param>
        public SignalRGroupAction(SignalRGroupActionType actionType)
        {
            Action = actionType;
        }

        /// <summary>
        /// Sets this field if you want to add a connection to a group or remove it from a group.
        /// </summary>
        public string? ConnectionId { get; set; }

        /// <summary>
        /// Sets this field if you want to add a user to a group or remove it from a group/all groups.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// The group to manage. Leave it null if you want to remove a user from all groups.
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// The group action type.
        /// </summary>
        public SignalRGroupActionType Action { get; set; }
    }

    /// <summary>
    /// The type of a group action.
    /// </summary>
    /// <remarks>
    /// The class will be available by extensions in the next release.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SignalRGroupActionType
    {
        /// <summary>
        /// Adds a user or a connection to a group.
        /// </summary>
        Add,
        /// <summary>
        /// Removes a user or a connection to a group.
        /// </summary>
        Remove,
        /// <summary>
        /// Remove a user from all the groups.
        /// </summary>
        RemoveAll
    }
}
