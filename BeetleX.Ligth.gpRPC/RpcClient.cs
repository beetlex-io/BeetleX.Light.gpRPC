using BeetleX.Light.Clients;
using BeetleX.Light.Protocols;
using BeetleX.Ligth.gpRPC.Messages;
using Google.Protobuf;
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
            ReturnSendDelay = false;
            RegisterMessages<RpcClient>();
            TimeOut = 1000000;
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

        public async Task<object> Request<REQ>(REQ message)
           where REQ : IMessage
        {
            RpcMessage req = new RpcMessage();
            req.Body = message;
            var result = (RpcMessage)await ((IAwaiterNetClient)this).Request(req);
            if (result.Body is Error err)
            {
                throw new Exception(err.ErrorMessage);
            }
            return result.Body;
        }
        public async Task<RESP> Request<REQ, RESP>(REQ message)
            where RESP : IMessage
            where REQ : IMessage
        {
            return (RESP)await Request<REQ>(message);
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
