using BeetleX.Light.Clients;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC.Messages;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class RpcClient : AwaiterNetClient<ProtobufChannel<NetClient>>
    {
        public RpcClient(string host, int port) : base(host, port)
        {
            ReturnSendDelay = false;
            RegisterMessages<RpcClient>();
            TimeOut = 1000000;
            Connecting = OnConnect;
        }

        public string UserName { get; set; }

        public string Password { get; set; }


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

        internal async Task<object> Request(IMessage message)
        {
            RpcMessage req = new RpcMessage();
            req.Body = message;
            var result = (RpcMessage)await ((IAwaiterNetClient)this).Request(req);
            if (result.Body is Error err)
            {
                throw new RpcException(err);
            }
            return result.Body;
        }
        internal async Task<RESP> Request<RESP>(IMessage message)
            where RESP : IMessage
        {
            return (RESP)await Request(message);
        }

        public Task Subscribe<T>()
        {
            return Subscribe(typeof(T));
        }

        public Task Subscribe<T, T1>()
        {
            return Subscribe(typeof(T), typeof(T1));
        }

        public Task Subscribe<T, T1, T2>()
        {
            return Subscribe(typeof(T), typeof(T1), typeof(T2));
        }

        public Task Subscribe<T, T1, T2, T3>()
        {
            return Subscribe(typeof(T), typeof(T1), typeof(T2), typeof(T3));
        }
        public Task Subscribe<T, T1, T2, T3, T4>()
        {
            return Subscribe(typeof(T), typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        }

        public async Task Subscribe(params Type[] types)
        {
            SubscribeReq req = new SubscribeReq();
            foreach (var item in types)
            {
                req.Items.Add(ProtocolMessageMapperFactory.UintMapper.GetTypeValue(item));
            }
            await Request(req);
        }

        private async Task OnConnect(NetClient client)
        {
            LoginReq req = new LoginReq();
            if (!string.IsNullOrEmpty(Password))
                req.Password = Password;
            if (!string.IsNullOrEmpty(UserName))
                req.UserName = UserName;
            var resp = await Request<Success>(req);
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
