using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class AckHandler : IAckHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, (TaskCompletionSource<AckResult>, DateTime)> _handlers
            = new ConcurrentDictionary<string, (TaskCompletionSource<AckResult>, DateTime)>();

        private readonly TimeSpan _ackThreshold;

        private Timer _timer;

        public AckHandler()
                    : this(completeAcksOnTimeout: true,
                           ackThreshold: TimeSpan.FromSeconds(10),
                           ackInterval: TimeSpan.FromSeconds(2))
        {
        }

        public AckHandler(bool completeAcksOnTimeout, TimeSpan ackThreshold, TimeSpan ackInterval)
        {
            if (completeAcksOnTimeout)
            {
                _timer = new Timer(_ => CheckAcks(), state: null, dueTime: ackInterval, period: ackInterval);
            }

            _ackThreshold = ackThreshold;
        }

        public AckInfo CreateAck()
        {
            var id = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<AckResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return new AckInfo(id, tcs.Task);
        }

        public void Ack(string id)
        {
            if (_handlers.TryGetValue(id, out var res))
            {
                res.Item1.TrySetResult(AckResult.Success);
            }
            else
            {
                throw new Exception("AckId not found");
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }

            foreach (var pair in _handlers)
            {
                pair.Value.Item1.TrySetCanceled();
            }
        }

        private void CheckAcks()
        {
            foreach (var pair in _handlers)
            {
                var elapsed = DateTime.UtcNow - pair.Value.Item2;
                if (elapsed > _ackThreshold)
                {
                    pair.Value.Item1.TrySetResult(AckResult.TimeOut);
                }
            }
        }
    }
}
