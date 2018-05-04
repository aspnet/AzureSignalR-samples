// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Timers;
using Interlocked = System.Threading.Interlocked;

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{
    class FlightRecord
    {
        public string Icao { get; set; }

        public long PosTime { get; set; }

        public double Lat { get; set; }

        public double Long { get; set; }
    }

    public class FlightControl : Hub, IFlightControl
    {
        private const int DefaultInterval = 1000;

        private readonly Timer timer;

        private int index = 0;

        private int totalVisitors = 0;

        private int speed = 1;

        private FlightRecord[][] flightData;

        private IHubContext<FlightMapHub> context;

        public FlightControl(IHubContext<FlightMapHub> context, IConfiguration configuration)
        {
            this.context = context;
            var dataUrl = configuration["DataFileUrl"];
            // TODO: make it async
            var client = new HttpClient();
            string data = client.GetStringAsync(dataUrl).GetAwaiter().GetResult();
            flightData = JsonConvert.DeserializeObject<FlightRecord[][]>(data);

            timer = new Timer(DefaultInterval / speed);
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

        public void Restart()
        {
            index = 0;
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void SetSpeed(int speed)
        {
            this.speed = speed;
            timer.Interval = DefaultInterval / speed;
        }

        public void BroadcastFlights(Object source, ElapsedEventArgs e)
        {
            var curr = flightData[index];
            var currTime = curr[0].PosTime;
            long serverTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            context.Clients.All.SendAsync("updateAircraft", DefaultInterval / speed, curr, index, serverTimestamp, currTime);
            index = (index + 1) % flightData.Length;
            Console.WriteLine($"Current index: {index}");
        }
    }
}
