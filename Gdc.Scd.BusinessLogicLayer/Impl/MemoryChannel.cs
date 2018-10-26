using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class MemoryChannel : INotifyChannel
    {
        private ConcurrentDictionary<string, List<object>> channels;

        public MemoryChannel()
        {
            channels = new ConcurrentDictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);
            new Thread(Push).Start();
        }

        void Push()
        {
            while (true)
            {
                Send(new { current_time = DateTime.Now, value = Guid.NewGuid() });
                Thread.Sleep(6000);
            }
        }

        public void Create(string username)
        {
            channels.TryAdd(username, new List<object>());
        }

        public void Send(object value)
        {
            foreach (var c in channels)
            {
                c.Value.Add(value);
            }
        }

        public void Send(string userName, object value)
        {
            List<object> list;
            if (channels.TryGetValue(userName, out list))
            {
                list.Add(value);
            }
        }

        public object GetMessage(string userName)
        {
            object result = null;

            List<object> list;
            if (channels.TryGetValue(userName, out list))
            {
                result = list.FirstOrDefault();
            }

            return result;
        }

        public void RemoveMessage(string userName, object msg)
        {
            List<object> list;
            if (channels.TryGetValue(userName, out list))
            {
                list.Remove(msg);
            }
        }
    }
}