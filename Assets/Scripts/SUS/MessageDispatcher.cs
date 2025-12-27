using System;
using System.Collections.Generic;

namespace SUS
{
    public class MessageDispatcher
    {
        private readonly Dictionary<Type, List<Delegate>> subscribers = new();

        public void Subscribe<T>(Action<T> callback) where T : IncomingPacket
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                subscribers[type] = list;
            }

            list.Add(callback);
        }

        public void Unsubscribe<T>(Action<T> callback) where T : IncomingPacket
        {
            var type = typeof(T);
            if (subscribers.TryGetValue(type, out var list))
            {
                list.Remove(callback);
            }
        }

        public void Dispatch(IncomingPacket message)
        {
            var type = message.GetType();

            if (!subscribers.TryGetValue(type, out var list))
            {
                return;
            }
            
            foreach (var del in list.ToArray())
            {
                del.DynamicInvoke(message);
            }
        }
    }
}