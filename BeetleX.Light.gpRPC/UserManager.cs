using BeetleX.Light.gpRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BeetleX.Light.gpRPC
{
    public class UserManager
    {
        public UserManager()
        {
            Create("admin", "123456");
        }


        public User Admin => GetUser("admin");

        public List<User> Users { get; private set; } = new List<User>();

        public User Create(string username, string password)
        {
            User user = new User { Name = username, Password = password };
            if (!Users.Contains(user))
            {
                Users.Add(user);
            }
            return Users.First(f => f.Name == username);
        }

        public User GetUser(string name)
        {
            return Users.FirstOrDefault(f => f.Name == name);
        }
    }
}
