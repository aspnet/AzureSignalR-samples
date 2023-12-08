using Microsoft.Azure.SignalR.Samples.ChatRoom.LLama;
using System.Collections.Generic;

namespace LLama.Web.Common
{
    public class LLamaOptions
    {
        public ModelLoadType ModelLoadType { get; set; }
        public ModelOptions Models { get; set; }

        public void Initialize()
        {
        }
    }
}
