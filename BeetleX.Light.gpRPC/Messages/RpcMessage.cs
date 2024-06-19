using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Messages
{
    public class RpcMessage : IIdentifier
    {

        public uint Type { get; set; }
        public UInt64 Identifier { get; set; }

        public object Body { get; set; }

    }
}
