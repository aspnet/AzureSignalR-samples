using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public interface IAckHandler
    {
        AckInfo CreateAck();
        void Ack(string id);
    }
}
