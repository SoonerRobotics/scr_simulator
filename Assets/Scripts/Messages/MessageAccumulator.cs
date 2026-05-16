using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace Messages
{
    public sealed class MessageAccumulator : IDisposable
    {
        private static readonly byte[] Magic = { 0x49, 0x47, 0x56, 0x43 };

        private const int MagicSize = 4;
        private const int TypeSize = 2;
        private const int LengthSize = 4;
        private const int FlagsSize = 2;
        private const int HeaderSize = MagicSize + TypeSize + LengthSize + FlagsSize; // 12
        private const int CrcSize = 4;

        private const ushort FlagCrc = 0x0001;

        private readonly Endianness _endianness;
        private readonly Action<MessageWrapper> _onMessage;
        private readonly int _maxMessageLength;
        private readonly int _maxBufferSize;

        private byte[] _buffer;
        private int _writePos;

        public MessageAccumulator(
            Endianness endianness,
            Action<MessageWrapper> onMessage,
            int initialCapacity = 4096,
            int maxMessageLength = 2 * 1024 * 1024,
            int maxBufferSize = 4 * 1024 * 1024
        )
        {
            _endianness = endianness;
            _onMessage = onMessage;
            _maxMessageLength = maxMessageLength;
            _maxBufferSize = maxBufferSize;
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        public void Append(ReadOnlySpan<byte> bytes)
        {
            if (_writePos + bytes.Length > _maxBufferSize)
            {
                throw new InvalidOperationException($"MessageAccumulator buffer overflow: {_writePos + bytes.Length} > {_maxBufferSize}");
            }

            EnsureCapacity(_writePos + bytes.Length);
            bytes.CopyTo(_buffer.AsSpan(_writePos));
            _writePos += bytes.Length;

            Process();
        }

        private void Process()
        {
            var readPos = 0;

            while (_writePos - readPos >= HeaderSize)
            {
                var span = _buffer.AsSpan(readPos, _writePos - readPos);

                if (!span[..MagicSize].SequenceEqual(Magic))
                {
                    readPos++;
                    continue;
                }

                var type = _endianness == Endianness.Little
                    ? BinaryPrimitives.ReadUInt16LittleEndian(span[MagicSize..])
                    : BinaryPrimitives.ReadUInt16BigEndian(span[MagicSize..]);

                var length = _endianness == Endianness.Little
                    ? BinaryPrimitives.ReadInt32LittleEndian(span[(MagicSize + TypeSize)..])
                    : BinaryPrimitives.ReadInt32BigEndian(span[(MagicSize + TypeSize)..]);

                if (length < 0 || length > _maxMessageLength)
                {
                    throw new InvalidOperationException($"Invalid message length {length}");
                }

                var flags = _endianness == Endianness.Little
                    ? BinaryPrimitives.ReadUInt16LittleEndian(span[(MagicSize + TypeSize + LengthSize)..])
                    : BinaryPrimitives.ReadUInt16BigEndian(span[(MagicSize + TypeSize + LengthSize)..]);

                var hasCrc = (flags & FlagCrc) != 0;
                var crcSize = hasCrc ? CrcSize : 0;
                var frameSize = HeaderSize + length + crcSize;

                if (_writePos - readPos < frameSize)
                {
                    break;
                }

                if (hasCrc)
                {
                    var computedCrc = Crc32.HashToUInt32(span[..(HeaderSize + length)]);
                    var receivedCrc = _endianness == Endianness.Little
                        ? BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(HeaderSize + length, CrcSize))
                        : BinaryPrimitives.ReadUInt32BigEndian(span.Slice(HeaderSize + length, CrcSize));

                    if (computedCrc != receivedCrc)
                    {
                        throw new InvalidOperationException($"CRC mismatch: computed {computedCrc:X8}, received {receivedCrc:X8}");
                    }
                }

                var payload = ArrayPool<byte>.Shared.Rent(length);
                span.Slice(HeaderSize, length).CopyTo(payload);
                _onMessage(MessageWrapper.FromPooled((MessageType)type, payload, length));

                readPos += frameSize;
            }

            if (readPos > 0)
            {
                var remaining = _writePos - readPos;
                if (remaining > 0)
                {
                    _buffer.AsSpan(readPos, remaining).CopyTo(_buffer);
                }

                _writePos = remaining;
            }
        }

        private void EnsureCapacity(int required)
        {
            if (_buffer.Length >= required)
            {
                return;
            }

            var newSize = Math.Max(_buffer.Length * 2, required);
            var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            _buffer.AsSpan(0, _writePos).CopyTo(newBuffer);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }
        }
    }
}