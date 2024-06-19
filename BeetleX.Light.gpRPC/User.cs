using BeetleX.Light.Protocols;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class User
    {
        public string Name { get; set; }

        public string Password { get; set; }

        private Dictionary<uint, uint> _right = new Dictionary<uint, uint>();

        public bool Check(uint messageType)
        {
            if (_right.Count == 0)
                return true;
            return _right.ContainsKey(messageType);
        }


        public override bool Equals(object? obj)
        {
            return this.Name.ToLower() == ((User)obj).Name.ToLower();
        }


        public User SetRight<T>()
            where T : IMessage
        {
            var value = ProtocolMessageMapperFactory.UintMapper.GetTypeValue(typeof(T));
            return SetRight(value);

        }
        public User SetRight(uint messageType)
        {
            _right[messageType] = messageType;
            return this;
        }
    }
}
