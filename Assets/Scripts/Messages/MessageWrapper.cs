using System;
using System.Runtime.InteropServices;
using Google.FlatBuffers;

namespace Messages
{
    public class MessageWrapper
    {
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }

        private ByteBuffer _buffer;
    
        public ByteBuffer GetByteBuffer()
        {
            if (!MemoryMarshal.TryGetArray(Data, out ArraySegment<byte> seg))
            {
                throw new InvalidOperationException("Non-array backed memory");
            }

            return new ByteBuffer(seg.Array!, seg.Offset);
        }
        
        public static MessageWrapper From(MessageType type, byte[] data)
        {
            return new MessageWrapper()
            {
                Type = type,
                Data = data
            };
        }
    }
}