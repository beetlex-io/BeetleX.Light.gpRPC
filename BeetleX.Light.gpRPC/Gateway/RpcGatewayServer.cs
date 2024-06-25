using BeetleX.Light;
using BeetleX.Light.Extension;
using BeetleX.Light.Protocols;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{

    public class RpcGatewayServer : NetServer<GatewayApplicatoin, RpcGatewaySession>
    {
        public string Host { get; set; }

        public int Port { get; set; } = 8080;

        public string TLSHost { get; set; }

        public int TLSPort { get; set; } = 8081;

        public string CertificateFile { get; set; }

        public UserManager UserManager { get; private set; } = new UserManager();

        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;

        public string CertificatePassword { get; set; }



        internal SafeIdGenerator SafeIdGenerator { get; set; } = new SafeIdGenerator(1u, 200000000u);

        public override void Start()
        {
            RegisterMessages<RpcClient>();
            Options.SynchronousIO = false;
            Options.SessionSingleIOQueue = false;
            Options.ServerName = "google protobuf rpc gateway";
            Options.ReturnSendDelay = true;
            Options.SetDefaultListen(o =>
            {
                o.Port = Port;
                o.Host = Host;
                o.SetProtocolChannel<RpcGatewayChannel<NetContext>>();
            });
            if (!string.IsNullOrEmpty(CertificateFile))
            {
                Options.SetListen("ssl", o =>
                {
                    o.Host = TLSHost;
                    o.Port = TLSPort;
                    o.EnabledSSL(CertificateFile, CertificatePassword,
                       SslProtocols);
                    o.SetProtocolChannel<RpcGatewayChannel<NetContext>>();
                });
            }
            base.Start();
            foreach (var t in _serviceTypes)
            {
                ServiceMethodHandlers.Default.Register(t, this);
            }
        }

        private List<System.Type> _serviceTypes = new List<System.Type>();

        internal MessageLoadBalancerFactory MessageLoadBalancerTable { get; set; } = new MessageLoadBalancerFactory();

        internal SubcribeReplyFactory SubcribeReplyTable { get; set; } = new SubcribeReplyFactory();

        public void RegisterMessages<T>()
        {
            ProtocolMessageMapperFactory.UintMapper.RegisterAssembly<T>(this);
            foreach (var type in typeof(T).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<RpcServiceAttribute>() != null)
                    _serviceTypes.Add(type);
            }
        }
    }
}
