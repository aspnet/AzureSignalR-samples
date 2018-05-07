// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.SignalR.Samples.FlightMap
{
    public interface IFlightControl {
        void Start();

        void Stop();

        void Restart();

        int VisitorJoin();

        int VisitorLeave();

        void SetSpeed(int speed);
    }
}
