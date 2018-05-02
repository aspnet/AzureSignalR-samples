// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Timers;
using System.Drawing;
using Interlocked = System.Threading.Interlocked;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{
    class FlightRecord
    {
        public string Icao;

        public long PosTime;

        public double Lat;

        public double Long;
    }

    public class FlightControl : Hub, IFlightControl
    {
        private const int DeviceInterval = 1000;

        private const int WorldInterval = 15 * 1000;

        private readonly Timer timer;

        private int index = 0;

        private int totalVisitors = 0;

        private FlightRecord[][] flightData;

        private IHubContext<FlightMapHub> context;

        public FlightControl(IHubContext<FlightMapHub> context, IConfiguration configuration)
        {
            this.context = context;
            var dataPath = configuration["DataFileUrl"];
            string data = File.ReadAllText(dataPath);
            flightData = JsonConvert.DeserializeObject<FlightRecord[][]>(data);

            timer = new Timer(DeviceInterval);
            timer.Elapsed += BroadcastFlights;
            Start();
        }

        public int VisitorJoin()
        {
            return Interlocked.Increment(ref totalVisitors);
        }

        public int VisitorLeave()
        {
            return Interlocked.Decrement(ref totalVisitors);
        }

        public void Start()
        {
            timer.Start();
        }

        public void Restart() {
            index = 0;
        }

        public void Stop() {
            timer.Stop();
        }

        public void BroadcastFlights(Object source, ElapsedEventArgs e) {
            var curr = flightData[index];
            var currTime = curr[0].PosTime;
            long serverTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            context.Clients.All.SendAsync("updateAircraft", DeviceInterval, curr, index, serverTimestamp, currTime, WorldInterval / DeviceInterval);
            index = (index + 1) % flightData.Length;
            Console.WriteLine($"Current index: {index}");
        }
    }
}
