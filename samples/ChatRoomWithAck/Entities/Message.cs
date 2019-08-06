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
        public DateTime sendTime { get; set; }

        public string ConvertMessageToJson(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public Message ConvertJsonToMessage(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }

        public Message(string Id, string SourceName, string TargetName, string Text, MessageType Type, DateTime sendTime)
        {
            this.Id = Id;
            this.Type = Type;
            this.SourceName = SourceName;
            this.TargetName = TargetName;
            this.Text = Text;
            this.sendTime = sendTime;
        }

        public Message(string SourceName, string TargetName, string Text, MessageType Type)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Type = Type;
            this.SourceName = SourceName;
            this.TargetName = TargetName;
            this.Text = Text;
        }
    }
}
