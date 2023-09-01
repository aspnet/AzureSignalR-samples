using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetForm
{
    public class Database
    {
        public static readonly Database Instance = new Database();

        private readonly Dictionary<int, NotificationItem> _dict = new Dictionary<int, NotificationItem>();
        private int _nextId = 1;

        public List<NotificationItem> GetItemsByGroup(string group)
        {
            lock (_dict)
            {
                return _dict.Values.Where(item => item.Group == group).ToList();
            }
        }

        public NotificationItem GetItemById(int id)
        {
            lock (_dict)
            {
                _dict.TryGetValue(id, out var item);
                return item;
            }
        }

        public NotificationItem Add(string group, string message)
        {
            lock (_dict)
            {
                var item = new NotificationItem
                {
                    Id = _nextId++,
                    Group = group,
                    SubmitAt = DateTime.UtcNow,
                    Message = message,
                    Status = "Submitted",
                };
                _dict.Add(item.Id, item);
                return item;
            }
        }

        public void SetProcessed(int id)
        {
            lock (_dict)
            {
                if (_dict.TryGetValue(id, out var item))
                {
                    item.Status = "Processed";
                    item.ProcessedAt = DateTime.UtcNow;
                }
            }
        }
    }
}