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
 
        static void RegisterType(string name,Type type)
        {
            if ((type == null) || dictionary.ContainsKey(name)) throw new NullReferenceException();
            dictionary.TryAdd(name, type);
        }
 
        static void Remove(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new NullReferenceException();
            dictionary.TryRemove(name, out var val);
        }
        
        public IMessageHandler Create(string type)
        {
            if ((type == null) || !dictionary.ContainsKey(type)) throw new NullReferenceException();
            Type targetType = dictionary[type];
            return (IMessageHandler)Activator.CreateInstance(targetType);
        }
}
}