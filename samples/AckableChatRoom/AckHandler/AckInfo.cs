using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.AckableChatRoom
{
    public class AckInfo
    {
        public string AckId { get; set; }

        public Task<string> AckTask { get; set; }

        public AckInfo(string ackId, Task<string> ackTask)
        {
            AckId = ackId;
            AckTask = ackTask;
        }
    }
}
