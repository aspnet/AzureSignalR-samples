// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace NegotiationServer
{
    // Copied from Message Publisher
    public interface IMessageClient
    {
        Task Target(string message);
    }
}