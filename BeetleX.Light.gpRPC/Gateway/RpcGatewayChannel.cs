using BeetleX.Light;
using BeetleX.Light.Memory;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC.Messages;
using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC.Gateway
{
    public class RpcGatewayChannel<T> : IProtocolChannel<T>
         where T : INetContext
    {
        public string Name => "GoogleProtobufGateway";

        public T Context { get; set; }

        public object Clone()
        {
            var result = new RpcGatewayChannel<T>();
            return result;
        }

        public void Encoding(IStreamWriter writer, object message)
        {
            writer.WriteBinaryObject(HeaderSizeType.UInt, message,
                (stream, msg) =>
                {
                    RpcMessage rpcMessage = msg as RpcMessage;
                    if (rpcMessage.Body is IMessage imsg)
                    {
                        rpcMessage.Type = ProtocolMessageMapperFactory.UintMapper.WriteType(stream, rpcMessage.Body, writer.LittleEndian);
                        stream.Write(rpcMessage.Identifier);
                        IMessage message1 = (IMessage)rpcMessage.Body;
                        message1.WriteTo(stream);
                    }
                    else
                    {
                        TemporaryBuffer<Byte> data = (TemporaryBuffer<Byte>)rpcMessage.Body;
                        stream.Write(rpcMessage.Type);
                        stream.Write(rpcMessage.Identifier);
                        stream.Write(data.Span);
                        data.Dispose();
                        Context.GetLoger(Logs.LogLevel.Debug)?.Write(Context, "gpRPCGateway", "ChannelEncoding", "Body memory disposed");
                    }
                });
        }

        public void Decoding(IStreamReader reader, Action<T, object> completed)
        {
            while (reader.TryReadBinaryObject(HeaderSizeType.UInt,
                    out object result, memory =>
                    {
                        RpcMessage rpcMessage = new RpcMessage();
                        var type = ProtocolMessageMapperFactory.UintMapper.ReadType(memory, reader.LittleEndian);
                        rpcMessage.Type = type.Value;
                        memory = memory.Slice(type.BuffersLength);
                        rpcMessage.Identifier = memory.Span.ReadUInt64();
                        memory = memory.Slice(8);
                        if (rpcMessage.Identifier <= uint.MaxValue && rpcMessage.Type < 2000000001u)
                        {
                            //var data = MemoryPool<byte>.Shared.Rent(memory.Length);
                            TemporaryBuffer<Byte> data = memory.Length;
                            memory.CopyTo(data.Memory);
                            rpcMessage.Body = data;
                            Context.GetLoger(Logs.LogLevel.Debug)?.Write(Context, "gpRPCGateway", "ChannelDecoding", "Read body to memory");
                        }
                        else
                        {
                            IMessage message = (IMessage)Activator.CreateInstance(type.MessageType);
                            message.MergeFrom(memory.Span);
                            rpcMessage.Body = message;

                        }
                        return rpcMessage;
                    })
                   )
            {
                completed(Context, result);
            }
        }

        public void Dispose()
        {

        }
    }
}
