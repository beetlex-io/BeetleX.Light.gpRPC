﻿using BeetleX.Light;
using BeetleX.Light.Memory;
using BeetleX.Light.Protocols;
using BeetleX.Light.gpRPC.Messages;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.Light.gpRPC
{
    public class ProtobufChannel<T> : IProtocolChannel<T>
         where T : INetContext
    {
        public string Name => "GoogleProtobuf";

        public T Context { get; set; }

        public object Clone()
        {
            var result = new ProtobufChannel<T>();
            return result;
        }

        public void Encoding(IStreamWriter writer, object message)
        {
            writer.WriteBinaryObject(HeaderSizeType.UInt, message,
                (stream, msg) =>
                {
                    RpcMessage rpcMessage = msg as RpcMessage;
                    rpcMessage.Type = ProtocolMessageMapperFactory.UintMapper.WriteType(stream, rpcMessage.Body, writer.LittleEndian);
                    stream.Write(rpcMessage.Identifier);
                    IMessage message1 = (IMessage)rpcMessage.Body;
                    message1.WriteTo(stream);
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
                        IMessage message = (IMessage)Activator.CreateInstance(type.MessageType);
                        message.MergeFrom(memory.Span);
                        rpcMessage.Body = message;
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
