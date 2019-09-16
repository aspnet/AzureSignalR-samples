using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR.Samples.AckableChatRoom
{
    public class AckHandler : IAckHandler, IDisposable
    {
        private readonly ConcurrentDictionary<string, (TaskCompletionSource<string>, DateTime)> _handlers
            = new ConcurrentDictionary<string, (TaskCompletionSource<string>, DateTime)>();

        private readonly TimeSpan _ackThreshold;

        private readonly Timer _timer;

        public AckHandler() : this(
            completeAcksOnTimeout: true,
            ackThreshold: TimeSpan.FromSeconds(5),
            ackInterval: TimeSpan.FromSeconds(1))
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
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return new AckInfo(id, tcs.Task);
        }

        public void Ack(string id)
        {
            if (_handlers.TryRemove(id, out var res))
            {
                res.Item1.TrySetResult("Sent");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();

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
                    pair.Value.Item1.TrySetException(new TimeoutException("Ack time out"));
                }
            }
        }
    }
}
