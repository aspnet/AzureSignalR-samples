using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class AckHandler : IAckHandler
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

        public (string, Task<AckResult>) CreateAck()
        {
            var id = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<AckResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return (id, tcs.Task);
        }

        public Task<AckResult> CreateAckWithId(string id)
        {
            var tcs = new TaskCompletionSource<AckResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return tcs.Task;
        }

        public void Ack(string id)
        {
            if (id.Equals(AckResult.NoAck.ToString()))
            {
                return;
            }

            if (_handlers.TryGetValue(id, out var res))
            {
                res.Item1.TrySetResult(AckResult.Success);
            }
            else
            {
                throw new NullReferenceException();
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
