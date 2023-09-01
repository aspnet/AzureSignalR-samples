using System;

namespace AspNetForm
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Group { get; set; }
        public DateTime SubmitAt { get; set; }
        public string Message { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Status { get; set; }
    }
}