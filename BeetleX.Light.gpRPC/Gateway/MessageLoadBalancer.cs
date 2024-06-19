using BeetleX.Light.gpRPC.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{
    internal class MessageLoadBalancer
    {

        public uint MessageType { get; set; }

        private System.Collections.Concurrent.ConcurrentDictionary<long, ContextItem> _Subscribers =
            new System.Collections.Concurrent.ConcurrentDictionary<long, ContextItem>();


        private ContextItem[] _Tables = new ContextItem[0];

        public void Add(NetContext context)
        {
            ContextItem item = new ContextItem();
            item.Context = context;
            _Subscribers[context.ID] = item;
            RefreshTable();
        }

        public void Remove(NetContext context)
        {
            _Subscribers.Remove(context.ID, out var result);
            RefreshTable();
        }

        private void RefreshTable()
        {
            var values = _Subscribers.Values.ToArray();
            if (values.Length == 0)
            {
                _Tables = new ContextItem[0];
            }
            int n = 100;
            ContextItem[] table = new ContextItem[n];

            double totalWeight = 0;
            foreach (var obj in values)
            {
                totalWeight += obj.Weight;
            }

            int currentIndex = 0;
            foreach (var obj in values)
            {
                int count = (int)Math.Round(obj.Weight / totalWeight * n);
                for (int i = 0; i < count && currentIndex < n; i++)
                {
                    table[currentIndex++] = obj;
                }
            }

            int remainingIndex = 0;
            while (currentIndex < n)
            {
                table[currentIndex++] = values[remainingIndex % values.Length];
                remainingIndex++;
            }
            _Tables = table;
        }

        private long _index;
        public ContextItem GetContext()
        {
            var index = System.Threading.Interlocked.Increment(ref _index);
            var table = _Tables;
            if (table.Length == 0)
                return null;
            return table[index % table.Length];
        }

        public class ContextItem
        {

            public NetContext Context { get; set; }

            public int Weight { get; set; } = 100;
        }
    }
}
