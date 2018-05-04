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
