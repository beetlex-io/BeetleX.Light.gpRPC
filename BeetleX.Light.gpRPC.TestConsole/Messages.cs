﻿using BeetleX.Light.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.TestConsole
{
    [ProtocolObject(101u)]
    public partial class RegisterReq
    {

    }

    [ProtocolObject(102u)]
    public partial class RegisterResp
    {

    }

    [ProtocolObject(201u)]
    public partial class UsersReq
    {

    }

    [ProtocolObject(202u)]
    public partial class UsersResp
    {

    }

    [ProtocolObject(203u)]
    public partial class SetTimeReq
    {

    }

    public interface IUserHandler
    {
        Task<RegisterResp> Register(RegisterReq req);

        Task<UsersResp> Users(UsersReq req);

        Task SetTime(SetTimeReq req);
    }
}
