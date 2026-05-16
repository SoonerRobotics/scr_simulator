using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace Messages
{
    public static class MessageWriter
    {
        private static readonly byte[] Magic = { 0x49, 0x47, 0x56, 0x43 };

        private const int MagicSize = 4;
        private const int TypeSize = 2;
        private const int LengthSize = 4;
        private const int FlagsSize = 2;
        private const int HeaderSize = MagicSize + TypeSize + LengthSize + FlagsSize; // 12
        private const int CrcSize = 4;

        private const ushort FlagCrc = 0x0001;

        public static (byte[] buffer, int length) WritePooled(
            MessageType type,
            ReadOnlySpan<byte> payload,
            Endianness endianness,
            bool includeCrc = true)
        {
            if (payload.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), $"Payload length {payload.Length} is invalid");
            }

            var crcSize = includeCrc ? CrcSize : 0;
            var frameSize = HeaderSize + payload.Length + crcSize;
            var buffer = ArrayPool<byte>.Shared.Rent(frameSize);
            var span = buffer.AsSpan(0, frameSize);

            WriteHeader(span, type, payload.Length, endianness, includeCrc);
            payload.CopyTo(span[HeaderSize..]);

            if (includeCrc)
            {
                var crc = Crc32.HashToUInt32(span[..(HeaderSize + payload.Length)]);
                if (endianness == Endianness.Little)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(span[(HeaderSize + payload.Length)..], crc);
                }
                else
                {
                    BinaryPrimitives.WriteUInt32BigEndian(span[(HeaderSize + payload.Length)..], crc);
                }
            }

            return (buffer, frameSize);
        }

        public static byte[] Write(
            MessageType type,
            ReadOnlySpan<byte> payload,
            Endianness endianness,
            bool includeCrc = true)
        {
            if (payload.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payload), $"Payload length {payload.Length} is invalid");
            }

            var crcSize = includeCrc ? CrcSize : 0;
            var frameSize = HeaderSize + payload.Length + crcSize;
            var buffer = new byte[frameSize];
            var span = buffer.AsSpan();

            WriteHeader(span, type, payload.Length, endianness, includeCrc);
            payload.CopyTo(span[HeaderSize..]);

            if (includeCrc)
            {
                var crc = Crc32.HashToUInt32(span[..(HeaderSize + payload.Length)]);
                if (endianness == Endianness.Little)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(span[(HeaderSize + payload.Length)..], crc);
                }
                else
                {
                    BinaryPrimitives.WriteUInt32BigEndian(span[(HeaderSize + payload.Length)..], crc);
                }
            }

            return buffer;
        }

        private static void WriteHeader(Span<byte> span, MessageType type, int payloadLength, Endianness endianness, bool includeCrc)
        {
            Magic.CopyTo(span);

            if (endianness == Endianness.Little)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(span[MagicSize..], (ushort)type);
                BinaryPrimitives.WriteInt32LittleEndian(span[(MagicSize + TypeSize)..], payloadLength);
                BinaryPrimitives.WriteUInt16LittleEndian(span[(MagicSize + TypeSize + LengthSize)..], includeCrc ? FlagCrc : (ushort)0);
            }
            else
            {
                BinaryPrimitives.WriteUInt16BigEndian(span[MagicSize..], (ushort)type);
                BinaryPrimitives.WriteInt32BigEndian(span[(MagicSize + TypeSize)..], payloadLength);
                BinaryPrimitives.WriteUInt16BigEndian(span[(MagicSize + TypeSize + LengthSize)..], includeCrc ? FlagCrc : (ushort)0);
            }
        }
    }
}