// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Microsoft.Azure.SignalR.Samples.SimpleEcho
{
    public class Client
    {
        private const string message = "Hello!";
        private readonly TaskCompletionSource<string> resp = new TaskCompletionSource<string>();

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => resp.TrySetCanceled());
            var connection = new HubConnectionBuilder().WithUrl("http://localhost:5000/echo").Build();
            connection.On<string>("echo", _resp => resp.TrySetResult(_resp));
            await connection.StartAsync(cancellationToken);
            await connection.InvokeAsync<string>("echo", message, cancellationToken);
            Console.WriteLine(await resp.Task);
        }
    }
}
