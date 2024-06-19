using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{
    internal class MessageLoadBalancerFactory
    {

        private System.Collections.Concurrent.ConcurrentDictionary<uint, MessageLoadBalancer> _messageLoadBalancers = new System.Collections.Concurrent.ConcurrentDictionary<uint, MessageLoadBalancer>();

        public MessageLoadBalancer GetMessageLoadBalancer(uint id)
        {
            if (!_messageLoadBalancers.TryGetValue(id, out var value))
            {
                value = new MessageLoadBalancer();
                value.MessageType = id;
                value = _messageLoadBalancers.GetOrAdd(id, value);
            }
            return value;
        }

        public void Add(uint type, NetContext context)
        {
            GetMessageLoadBalancer(type).Add(context);
        }

        public void Remove(NetContext context)
        {

            foreach (var item in _messageLoadBalancers.Values)
            {
                item.Remove(context);
            }
        }
    }
}
