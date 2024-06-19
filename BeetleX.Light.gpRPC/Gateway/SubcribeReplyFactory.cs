using BeetleX.Light.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{
    internal class SubcribeReplyFactory
    {

        private System.Collections.Concurrent.ConcurrentDictionary<ulong, ReplyItem> _ReplyItems = new System.Collections.Concurrent.ConcurrentDictionary<ulong, ReplyItem>();

        public void SetReplyContext(ulong id, NetContext context)
        {
            ReplyItem item = new ReplyItem();
            item.Context = context;
            item.StartTime = TimeWatch.GetTotalSeconds();
            _ReplyItems[id] = item;

        }

        public NetContext GetReplyContext(ulong id)
        {
            ReplyItem result = null;
            _ReplyItems.TryGetValue(id, out result);
            return result?.Context;
        }



        public class ReplyItem
        {
            public NetContext Context { get; set; }

            public double StartTime { get; set; }

        }
    }
}
