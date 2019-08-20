using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.ReliableChatRoom
{
    public interface IAckHandler
    {
        AckInfo CreateAck();
        void Ack(string id);
    }
}
