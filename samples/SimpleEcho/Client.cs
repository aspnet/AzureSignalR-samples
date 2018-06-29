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

        public async Task Run()
        {
            try
            {
                var connection = new HubConnectionBuilder().WithUrl("http://localhost:5000/echo").Build();
                connection.On<string>("echo", _resp => resp.SetResult(_resp));
                await connection.StartAsync();
                await connection.InvokeAsync<string>("echo", message);
                Console.WriteLine(await resp.Task);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw;
            }
        }
    }
}
