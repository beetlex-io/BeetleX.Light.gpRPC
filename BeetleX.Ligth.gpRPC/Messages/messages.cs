using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC.Messages
{
    [ProtocolObject(1000000u)]
    public partial class Error
    {
    }
    [ProtocolObject(1000001u)]
    public partial class LoginReq
    {
    }
    [ProtocolObject(1000002u)]
    public partial class LoginResp
    {
    }
}
