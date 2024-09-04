using System.Collections.Concurrent;

namespace AIStreaming
{
    public class GroupAccessor
    {
        private readonly ConcurrentDictionary<string, string> _store = new();

        public void Join(string connectionId, string groupName)
        {
            _store.AddOrUpdate(connectionId, groupName, (key, value) => groupName);
        }

        public void Leave(string connectionId)
        {
            _store.TryRemove(connectionId, out _);
        }

        public bool TryGetGroup(string connectionId, out string? group)
        {
            return _store.TryGetValue(connectionId, out group);
        }
    }
}
