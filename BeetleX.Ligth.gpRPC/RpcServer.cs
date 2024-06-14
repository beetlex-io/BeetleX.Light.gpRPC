using BeetleX.Light;
using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BeetleX.Ligth.gpRPC.RpcSession;

namespace BeetleX.Ligth.gpRPC
{
    public class RpcServer<APPLICATION> : NetServer<APPLICATION, RpcSession>
        where APPLICATION : IApplication, new()
    {
        public override void Start()
        {
            Options.SynchronousIO = false;
            Options.SessionSingleIOQueue = false;
            Options.ServerName = "google protobuf rpc";
            Options.ReturnSendDelay = true;
            Options.SetDefaultListen(o =>
            {
                o.SetProtocolChannel<ProtobufChannel<NetContext>>();
            });
            base.Start();
            foreach (var t in _serviceTypes)
            {
                RpcSession.MessageSessionHandlers.Default.Register(t, this);
            }
        }

        private List<Type> _serviceTypes = new List<Type>();

        public void RegisterMessages<T>()
        {
            ProtocolMessageMapperFactory.UintMapper.RegisterAssembly<T>();
            foreach (var type in typeof(T).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<RpcServiceAttribute>() != null)
                    _serviceTypes.Add(type);
            }
        }
    }
}
