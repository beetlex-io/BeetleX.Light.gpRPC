using BeetleX.Light.gpRPC.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class RpcException : Exception
    {
        public RpcException(Error error) : base(error.ErrorMessage)
        {
            ErrorCode = error.ErrorCode;


        }

        public const uint METHOD_NOTFOUND = 0X001;

        public const uint METHOD_INVOKE_ERROR = 0X002;

        public const uint SUBSCRIBER_NOT_SUPPORT = 0X003;

        public const uint INVALID_NAME_OR_PASSWORD = 0X004;

        public const uint SUBSCRIBER_ERROR = 0X005;

        public const uint INVALID_CONNECTION = 0X006;


        public const uint SERVICE_UNAVAILABLE = 0X007;
        public uint ErrorCode { get; private set; }
        public RpcException(string message) : base(message) { }

        public RpcException(string message, params object[] parameters) : base(string.Format(message, parameters)) { }

        public RpcException(string message, Exception baseError) : base(message, baseError) { }

        public RpcException(Exception baseError, string message, params object[] parameters) : base(string.Format(message, parameters), baseError) { }
    }
}
