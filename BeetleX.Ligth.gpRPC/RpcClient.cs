using BeetleX.Light.Clients;
using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC
{
    public class RpcClient : AwaiterNetClient<ProtobufChannel<NetClient>>
    {
        public RpcClient(string host, int port) : base(host, port)
        {
            ReturnSendDelay = true;
        }
        public static implicit operator RpcClient((string, int) info)
        {
            var NetClient = new RpcClient(info.Item1, info.Item2);
            return NetClient;
        }

        public static implicit operator RpcClient(string uri)
        {
            Uri uriInfo = new Uri(uri);
            var NetClient = new RpcClient(uriInfo.Host, uriInfo.Port);
            return NetClient;
        }
        public void RegisterMessages<T>()
        {
            ProtocolMessageMapperFactory.UintMapper.RegisterAssembly<T>();
        }
        public T Create<T>()
        {
            if (!typeof(T).IsInterface)
            {
                throw new Exception($"{typeof(T).Name} not is interface!");
            }
            object result = DispatchProxy.Create<T, RpcInterfaceProxy>();
            RpcInterfaceProxy dispatch = ((RpcInterfaceProxy)result);
            dispatch.RpcClient = this;
            dispatch.Type = typeof(T);
            dispatch.InitHandlers();
            return (T)result;
        }
    }
}
