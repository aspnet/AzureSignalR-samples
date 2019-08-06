using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class Assembler
    {
        private static readonly ConcurrentDictionary<string, Type> Dictionary = new ConcurrentDictionary<string, Type>();

        static Assembler()
        {
            Dictionary.TryAdd("StaticMessageStorage", typeof(StaticMessageStorage));
        }

        public IMessageHandler Create(string type)
        {
            if ((type == null) || !Dictionary.ContainsKey(type)) throw new NullReferenceException();
            Type targetType = Dictionary[type];
            return (IMessageHandler)Activator.CreateInstance(targetType);
        }
    }
}
