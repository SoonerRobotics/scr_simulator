using System;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace Messages
{
    public static class MessageWriter
    {
        private static readonly byte[] Magic = { 0x49, 0x47, 0x56, 0x43 };
        private const int MagicSize = 4;
        private const int HeaderSize = MagicSize + 6;
        private const int CrcSize = 4;

        public static byte[] Write(
            MessageType type,
            ReadOnlySpan<byte> payload,
            Endianness endianness,
            int maxMessageLength = 64 * 1024)
        {
            if (payload.Length <= 0 || payload.Length > maxMessageLength)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), $"Payload length {payload.Length} is invalid");
            }

            var frameSize = HeaderSize + payload.Length + CrcSize;
            var buffer = new byte[frameSize];
            var span = buffer.AsSpan();

            Magic.CopyTo(span);
            
            if (endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(span[MagicSize..], (ushort)type);
                BinaryPrimitives.WriteInt32LittleEndian(span[(MagicSize + 2)..], payload.Length);
            }
            else
            {
                BinaryPrimitives.WriteUInt16BigEndian(span[MagicSize..], (ushort)type);
                BinaryPrimitives.WriteInt32BigEndian(span[(MagicSize + 2)..], payload.Length);
            }


            payload.CopyTo(span[HeaderSize..]);
            var crc = Crc32.HashToUInt32(span[..(HeaderSize + payload.Length)]);
            BinaryPrimitives.WriteUInt32LittleEndian(span[(HeaderSize + payload.Length)..], crc);

            return buffer;
        }
    }
}