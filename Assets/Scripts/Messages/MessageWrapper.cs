using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Google.FlatBuffers;

namespace Messages
{
    public sealed class MessageWrapper : IDisposable
    {
        public MessageType Type { get; private set; }
        public byte[] Data { get; private set; }
        public int Length { get; private set; }

        private bool _pooled;
        private int _disposed;

        public ByteBuffer GetByteBuffer()
        {
            if (!MemoryMarshal.TryGetArray<byte>(Data, out var seg))
                throw new InvalidOperationException("Non-array backed memory");
            return new ByteBuffer(seg.Array!, seg.Offset);
        }

        public static MessageWrapper From(MessageType type, byte[] data)
        {
            return new MessageWrapper
            {
                Type = type,
                Data = data,
                Length = data.Length,
                _pooled = false
            };
        }

        public static MessageWrapper FromPooled(MessageType type, byte[] rentedArray, int length)
        {
            return new MessageWrapper
            {
                Type = type,
                Data = rentedArray,
                Length = length,
                _pooled = true
            };
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 1) return;
            if (_pooled)
            {
                ArrayPool<byte>.Shared.Return(Data);
            }
        }
    }
}