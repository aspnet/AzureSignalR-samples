// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.Management
{
    public interface IMessagePublisher
    {
        Task<bool> CheckExist(string type, string id);
        Task CloseConnection(string connectionId, string reason);
        Task DisposeAsync();
        Task InitAsync();
        Task ManageUserGroup(string command, string userId, string groupName);
        Task SendMessages(string command, string receiver, string message);
    }
}