// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace SignalRClient
{
    internal class BufferConnection
    {
        internal HubConnection conn;
        internal Queue<string> buffer; // Used to store message received from new connection before old connection ending
        internal bool active; // Is this connection has already been active or not.
        internal string addr; // Connected service's url
        internal int finish; // A flag represents finish state, 2 means both two sides have received barrier msg
 
        internal BufferConnection(string url)
        {
            addr = url;
            conn = new HubConnectionBuilder().WithUrl(url).Build();
            buffer = new Queue<string>();
            active = true;
            finish = 0;
        }

        internal BufferConnection(string url, Action<Microsoft.AspNetCore.Http.Connections.Client.HttpConnectionOptions> opt)
        {
            addr = url;
            conn = new HubConnectionBuilder().WithUrl(url, opt).WithAutomaticReconnect().Build();
            buffer = new Queue<string>();
            active = false;
            finish = 0;
        }

    }
    internal class StableConnection
    {
        private BufferConnection curconn;
        private Queue<BufferConnection> conns; // Backup connections, there may be consecutive reloading event
        private Channel<string> chan;
        private Thread t;
        private const string DefaultHubEndpoint = "http://localhost:5000/ManagementSampleHub";
        private const string Target = "Target";
        private const string DefaultUser = "User";

        internal StableConnection(string hubEndpoint = DefaultHubEndpoint, string userId = DefaultUser)
        {
            var url = hubEndpoint.TrimEnd('/') + $"?user={userId}";
            
            curconn = new BufferConnection(url);
            
            Bind(curconn);

            conns = new Queue<BufferConnection>();
            chan = Channel.CreateUnbounded<string>();

            t = new Thread(() => readFromChannel(DefaultUser));
            t.Start();
        }

        private async void readFromChannel(string userId)
        {
            // print received messages if any
            while (await chan.Reader.WaitToReadAsync())
                while (chan.Reader.TryRead(out string item))
                    Console.WriteLine($"{userId}: gets message from {curconn.addr}: '{item}'");
        }

        private void Bind(BufferConnection bc)
        {
            bc.conn.On(Target, async (string message) =>
            {
                Message msg = JsonConvert.DeserializeObject<Message>(message);

                // 0 : Messages
                // 1 : ReloadMessage
                // 2 : FinMessage
                // 3 : AckMessage
                if (msg.msgType == "0" && bc.active)
                {
                    if (bc.active) chan.Writer.TryWrite(msg.content);
                    else
                    {
                        // If not active yet, put message to the buffer
                        bc.buffer.Enqueue(msg.content);
                    }
                }
                else if (msg.msgType == "1")
                {
                    ReloadMessage rmsg = JsonConvert.DeserializeObject<ReloadMessage>(msg.content);
                    Console.WriteLine("My url is" + rmsg.url);
                    BufferConnection backup_conn = new BufferConnection(rmsg.url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(rmsg.token);
                    });
                    conns.Enqueue(backup_conn);

                    Bind(backup_conn);

                    await backup_conn.conn.StartAsync();
                    // Send barrier msg with endConnID to both old and new conns
                    await bc.conn.SendAsync("Barrier", bc.conn.ConnectionId, backup_conn.conn.ConnectionId);
                    await backup_conn.conn.SendAsync("Barrier", bc.conn.ConnectionId, backup_conn.conn.ConnectionId);
                } else if (msg.msgType == "2")
                {
                    bc.active = false;
                    FinMessage fmsg = JsonConvert.DeserializeObject<FinMessage>(msg.content);

                    if (fmsg.from == bc.conn.ConnectionId)
                    {
                        bc.finish++;
                    }
                } else if (msg.msgType == "3")
                {
                    Console.WriteLine(bc.conn.ConnectionId + " Received message 3");
                    AckMessage amsg = JsonConvert.DeserializeObject<AckMessage>(msg.content);
                    if (amsg.connID == bc.conn.ConnectionId)
                    {
                        bc.finish++;
                    }
                }

                // If both client and old service have received each other's barrier message
                if (bc.finish == 2)
                {
                    if (conns.Count == 0) return;
                    curconn = conns.Dequeue();
                    // Write new connection's buffering messages to the channel
                    while (curconn.buffer.Count>0) chan.Writer.TryWrite(curconn.buffer.Dequeue());
                    curconn.active = true;
                    await bc.conn.StopAsync();

                }
            });

            bc.conn.Closed += ex =>
            {
                Console.WriteLine(ex);
                return Task.CompletedTask;
            };
        }

        internal async Task StartAsync()
        {
            await curconn.conn.StartAsync();
        }

        internal async Task StopAsync()
        {
            await curconn.conn.StopAsync();
        }
    }
}