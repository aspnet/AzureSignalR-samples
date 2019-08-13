using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class AckInfo
    {
        public string AckId { get; set; }

        public Task<AckResult> AckTask { get; set; }

        public AckInfo(string ackId, Task<AckResult> ackTask)
        {
            this.AckId = ackId;
            this.AckTask = ackTask;
        }
    }
}
