using BeetleX.Light.Protocols;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class User
    {
        public string Name { get; set; }

        public string Password { get; set; }

        private Dictionary<uint, uint> _whitelist = new Dictionary<uint, uint>();

        public Dictionary<uint, uint> WhiteList
        {
            get
            {
                return _whitelist;
            }
            set
            {
                if (value != null)
                    _whitelist = value;
            }
        }


        private Dictionary<uint, uint> _blacklist = new Dictionary<uint, uint>();

        public Dictionary<uint, uint> BlackList
        {
            get
            {
                return _blacklist;
            }
            set
            {
                if (value != null)
                    _blacklist = value;
            }
        }

        public bool Check(uint messageType)
        {
            if (_whitelist.Count == 0 && _blacklist.Count == 0)
                return true;
            if (_whitelist.Count > 0)
            {
                return _whitelist.ContainsKey(messageType);
            }
            else
            {
                return !_blacklist.ContainsKey(messageType);
            }
        }


        public override bool Equals(object? obj)
        {
            return this.Name.ToLower() == ((User)obj).Name.ToLower();
        }

        public User SetWhite<T>()
            where T : IMessage
        {
            var value = ProtocolMessageMapperFactory.UintMapper.GetTypeValue(typeof(T));
            return SetWhite(value);

        }
        public User SetWhite(uint messageType)
        {
            _whitelist[messageType] = messageType;
            return this;
        }

        public User SetBlack<T>()
           where T : IMessage
        {
            var value = ProtocolMessageMapperFactory.UintMapper.GetTypeValue(typeof(T));
            return SetBlack(value);

        }
        public User SetBlack(uint messageType)
        {
            _blacklist[messageType] = messageType;
            return this;
        }


    }
}
