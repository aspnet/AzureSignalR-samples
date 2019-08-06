using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class AckHandler
    {
        private static ConcurrentDictionary<string, (TaskCompletionSource<string>, DateTime)> _handlers
            = new ConcurrentDictionary<string, (TaskCompletionSource<string>, DateTime)>();

        private TimeSpan _ackThreshold;

        private Timer _timer;
        public AckHandler()
                    : this(completeAcksOnTimeout: true,
                           ackThreshold: TimeSpan.FromSeconds(10),
                           ackInterval: TimeSpan.FromSeconds(2))
        {
        }


        public (string, Task<String>) CreateAck()
        {
            var id = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return (id, tcs.Task);
        }

        public Task<String> CreateAckWithId(string id)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            _handlers.TryAdd(id, (tcs, DateTime.UtcNow));
            return tcs.Task;
        }

        public AckHandler(bool completeAcksOnTimeout, TimeSpan ackThreshold, TimeSpan ackInterval)
        {
            if (completeAcksOnTimeout)
            {
                _timer = new Timer(_ => CheckAcks(), state: null, dueTime: ackInterval, period: ackInterval);
            }

            _ackThreshold = ackThreshold;
        }

        private void CheckAcks()
        {
            foreach (var pair in _handlers)
            {
                TimeSpan elapsed = DateTime.UtcNow - pair.Value.Item2;
                if (elapsed > _ackThreshold)
                {
                    pair.Value.Item1.TrySetResult(AckResult.TimeOut);
                }
            }
        }

        public string Ack(string id)
        {
            if (id.Equals(AckResult.NoAck))
            {
                return AckResult.Success;
            }
            if (_handlers.TryGetValue(id, out var res))
            {
                res.Item1.TrySetResult(AckResult.Success);
                return AckResult.Success;
            }
            return AckResult.Fail;
        }

        public void TimeOutCheck()
        {
            foreach (var pair in _handlers)
            {
                pair.Value.Item1.TrySetCanceled();
            }
        }
    }
}
