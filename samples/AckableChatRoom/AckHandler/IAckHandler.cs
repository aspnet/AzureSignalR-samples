using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Samples.AckableChatRoom
{
    public interface IAckHandler
    {
        AckInfo CreateAck();

        void Ack(string id);
    }
}
