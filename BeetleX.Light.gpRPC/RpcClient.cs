using BeetleX.Light.Clients;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC.Messages;
using BeetleX.Light.Logs;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BeetleX.Light.Dispatchs;
using System.Collections;

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

        public override bool OnReceiveMessage(IAwaiterNetClient client, object msg)
        {
            if (msg is RpcMessage gpMsg)
            {
                var handler = ServiceMethodHandlers.Default.GetMethod(gpMsg.Body.GetType());
                if (handler != null)
                {
                    IOQueue queue;
                    if (gpMsg.Body is IConsistency consistency)
                    {
                        queue = IOQueueFactory.Default.GetIOQueue(consistency.ConsistencyID);
                    }
                    else
                    {
                        queue = IOQueueFactory.Default.NextIOQueue();
                    }
                    MessageInvokdWork work = new MessageInvokdWork();
                    work.Message = gpMsg;
                    work.Client = this;
                    work.Handler = handler;
                    queue.Schedule(work);
                    GetLoger(LogLevel.Debug)?.Write(this, "gpRPCClient", "Receive", $"Message schedule to {queue.ID}");
                    return true;
                }
            }
            return base.OnReceiveMessage(client, msg);
        }

        class MessageInvokdWork : IIOWork
        {
            public RpcMessage Message { get; set; }

            public RpcClient Client { get; set; }

            public MethodInvokeHandler Handler { get; set; }

            public async Task Execute()
            {

                RpcMessage resp = new RpcMessage();
                resp.Identifier = Message.Identifier;

                try
                {
                    Task task = (Task)Handler.Method.Invoke(Handler.Service, new object[] { Message.Body });
                    await task;
                    var result = Handler.ResultProperty.GetValue(task);
                    resp.Body = result;
                    Client.GetLoger(LogLevel.Debug)?.Write(Client, "gpRPCClient", "InvokeSuccess", $"{Message.Body.GetType().Name}");

                }
                catch (Exception e_)
                {
                    Error error = new Error();
                    error.ErrorCode = RpcException.METHOD_INVOKE_ERROR;
                    error.ErrorMessage = e_.Message;
                    error.StackTrace = e_.StackTrace;
                    resp.Body = error;
                    Client?.GetLoger(LogLevel.Error)?.Write(Client, "gpRPCClient", "InvokeError", $"{Message.Body.GetType().Name} invok error {e_.Message} {e_.StackTrace}!");
                }

                Client?.Send(resp);
            }
        }


        public void RegisterMessages<T>()
        {
            ProtocolMessageMapperFactory.UintMapper.RegisterAssembly<T>();
            foreach (var type in typeof(T).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<RpcServiceAttribute>() != null)
                    ServiceMethodHandlers.Default.Register(type, this);
            }
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
