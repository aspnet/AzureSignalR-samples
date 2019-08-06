using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class Message
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public MessageType Type { get; set; }
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public DateTime SendTime { get; set; }

        public string ConvertMessageToJson(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public Message ConvertJsonToMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }

        public Message(string id, string sourceName, string targetName, string text, MessageType type, DateTime sendTime)
        {
            this.Id = id;
            this.Type = type;
            this.SourceName = sourceName;
            this.TargetName = targetName;
            this.Text = text;
            this.SendTime = sendTime;
        }

        public Message(string sourceName, string targetName, string text, MessageType type)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Type = type;
            this.SourceName = sourceName;
            this.TargetName = targetName;
            this.Text = text;
        }
    }
}
