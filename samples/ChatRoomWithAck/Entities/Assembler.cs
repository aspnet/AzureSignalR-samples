using System;
using System.Collections.Concurrent;

namespace Microsoft.Azure.SignalR.Samples.ChatRoomWithAck
{
    public class Assembler
    {
        private static ConcurrentDictionary<string, Type> dictionary = new ConcurrentDictionary<string, Type>();

        static Assembler()
        {
            dictionary.TryAdd("StaticMessageStorage", typeof(StaticMessageStorage));
        }

        public IMessageHandler Create(string type)
        {
            if ((type == null) || !dictionary.ContainsKey(type)) throw new NullReferenceException();
            Type targetType = dictionary[type];
            return (IMessageHandler)Activator.CreateInstance(targetType);
        }
    }
}
