﻿using BeetleX.Light;
using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using static BeetleX.Light.gpRPC.RpcSession;

namespace BeetleX.Light.gpRPC
{
    public class RpcServer : NetServer<RpcApplication, RpcSession>
    {
        public string Host { get; set; }

        public int Port { get; set; } = 8080;

        public string TLSHost { get; set; }

        public int TLSPort { get; set; } = 8081;

        public string CertificateFile { get; set; }

        public UserManager UserManager { get; set; } = new UserManager();

        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;

        public string CertificatePassword { get; set; }

        public override void Start()
        {
            RegisterMessages<RpcClient>();
            Options.SynchronousIO = false;
            Options.SessionSingleIOQueue = false;
            Options.ServerName = "google protobuf rpc server";
            Options.ReturnSendDelay = true;
            Options.SetDefaultListen(o =>
            {
                o.Port = Port;
                o.Host = Host;
                o.SetProtocolChannel<ProtobufChannel<NetContext>>();
            });
            if (!string.IsNullOrEmpty(CertificateFile))
            {
                Options.SetListen("tls", o =>
                {
                    o.Host = TLSHost;
                    o.Port = TLSPort;
                    o.EnabledSSL(CertificateFile, CertificatePassword,
                        SslProtocols);
                    o.SetProtocolChannel<ProtobufChannel<NetContext>>();
                });
            }
            base.Start();
            foreach (var t in _serviceTypes)
            {
                ServiceMethodHandlers.Default.Register(t, this);
            }
        }

        private List<Type> _serviceTypes = new List<Type>();

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
