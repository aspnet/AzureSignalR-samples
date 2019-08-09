using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public interface IAckHandler
    {
        (string, Task<AckResult>) CreateAck();

        Task<AckResult> CreateAckWithId(string id);

        void Ack(string id);
    }
}
