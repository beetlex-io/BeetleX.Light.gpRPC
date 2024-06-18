using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC.Messages
{
    public class RpcMessage : IIdentifier
    { 

        public uint Identifier { get; set; }

        public object Body { get; set; }

    }
}
