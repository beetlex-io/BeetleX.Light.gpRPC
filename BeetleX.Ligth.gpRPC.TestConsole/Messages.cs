using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Ligth.gpRPC.TestConsole
{
    [ProtocolObject(101u)]
    public partial class RegisterReq : IIdentifier
    {

    }

    [ProtocolObject(102u)]
    public partial class RegisterResp : IIdentifier
    {

    }

    [ProtocolObject(201u)]
    public partial class UsersReq : IIdentifier
    {

    }

    [ProtocolObject(202u)]
    public partial class UsersResp : IIdentifier
    {

    }

    public interface IUserHandler
    {
        Task<RegisterResp> Register(RegisterReq req);

        Task<UsersResp> Users(UsersReq req);
    }
}
