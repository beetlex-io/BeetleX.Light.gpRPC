using BeetleX.Light.Protocols;

namespace gpRPC.Gateway.Messages
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
    public partial class SearchUserReq
    {

    }

    [ProtocolObject(202u)]
    public partial class SearchUserResp
    {

    }

    [ProtocolObject(203u)]
    public partial class SetTimeReq
    {

    }

    public interface IUserHandler
    {
        Task<RegisterResp> Register(RegisterReq req);

        Task<SearchUserResp> Search(SearchUserReq req);

        Task SetTime(SetTimeReq req);
    }
}
