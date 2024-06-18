using BeetleX.Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{
    public class RpcGatewaySession : ISession
    {
        public string Name { get; set; }
        public AuthenticationType Authentication { get; set; }

        public RpcGatewayServer Sever { get; set; }

        public GatewayApplicatoin Application { get; set; }

        public void Connected(NetContext context)
        {
            this.Sever = (RpcGatewayServer)context.Server;
            this.Application = (GatewayApplicatoin)context.Server.Application;
        }

        public void Dispose(NetContext context)
        {

        }

        public Task Receive(NetContext context, object message)
        {
            return Task.CompletedTask;
        }

        public void ReceiveCompleted(int bytes)
        {

        }

        public void SendCompleted(int bytes)
        {

        }
    }
}
