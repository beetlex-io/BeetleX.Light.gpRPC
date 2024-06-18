using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gpRPC.Messages
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

    public interface IUserHandler
    {
        Task<RegisterResp> Register(RegisterReq req);

        Task<UsersResp> Users(UsersReq req);
    }

    [RpcService]
    public class UserHandler : IUserHandler
    {
        public async Task<RegisterResp> Register(RegisterReq req)
        {
            RegisterResp resp = new RegisterResp();
            resp.Success = true;
            resp.Time = DateTime.Now.Ticks;
            return resp;
        }

        public async Task<UsersResp> Users(UsersReq req)
        {
            UsersResp resp = new UsersResp();
            for (int i = 0; i < req.Count; i++)
            {
                User user = new User();
                user.Address = $"guangzhouLongdong{i}";
                user.City = $"guzngzhou{i}";
                user.Email = "henryfan@msn.com";
                user.FirstName = $"fan{i}";
                user.LastName = $"henry{i}";
                user.Password = "122".PadLeft(i, 'a');
                user.Remark = $"{i}";
                resp.Items.Add(user);
            }
            return resp;
        }
    }
}
