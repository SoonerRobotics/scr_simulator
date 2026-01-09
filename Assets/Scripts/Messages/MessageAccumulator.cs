using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Hashing;

namespace Messages
{
    /// <summary>
    /// Accumulates raw bytes and extracts framed messages.
    ///
    /// Format:
    /// [ ushort type ][ int length ][ payload ][ uint crc32c ]
    ///  
    /// CRC covers the (type + length + payload)
    /// </summary>
    public sealed class MessageAccumulator
    {
        private static readonly byte[] Magic = { 0x49, 0x47, 0x56, 0x43 };
        private readonly MemoryStream _stream;

        private const int MagicSize = 4;
        private const int HeaderSize = MagicSize + 6;
        private const int CrcSize = 4;

        private readonly Endianness _endianness;
        private readonly Action<MessageWrapper> _onMessage;
        private readonly int _maxMessageLength;
        private readonly int _maxBufferSize;

        public MessageAccumulator(
            Endianness endianness,
            Action<MessageWrapper> onMessage,
            int initialCapacity = 4096,
            int maxMessageLength = 512 * 1024, // 512 KB
            int maxBufferSize = 1024 * 1024 // 1024 KB
        )
        {
            _endianness = endianness;
            _onMessage = onMessage;
            _maxMessageLength = maxMessageLength;
            _maxBufferSize = maxBufferSize;

            _stream = new MemoryStream(initialCapacity);
        }

        public void Append(ReadOnlySpan<byte> bytes)
        {
            if (_stream.Length + bytes.Length > _maxBufferSize)
            {
                throw new InvalidOperationException("MessageAccumulator buffer overflow");
            }

            _stream.Write(bytes);
            Process();
        }

        private void Process()
        {
            _stream.Position = 0;

            while (_stream.Length - _stream.Position >= HeaderSize)
            {
                var buffer = _stream.GetBuffer();
                var span = buffer.AsSpan((int)_stream.Position);

                if (!span[..MagicSize].SequenceEqual(Magic))
                {
                    // skip until magic
                    _stream.Position += 1;
                    continue;
                }
                
                var type = _endianness == Endianness.Little
                    ? BinaryPrimitives.ReadUInt16LittleEndian(span[MagicSize..])
                    : BinaryPrimitives.ReadUInt16BigEndian(span[MagicSize..]);
                var length = _endianness == Endianness.Little
                    ? BinaryPrimitives.ReadInt32LittleEndian(span[(MagicSize + 2)..])
                    : BinaryPrimitives.ReadInt32BigEndian(span[(MagicSize + 2)..]);

                if (length < 0 || length >= _maxMessageLength)
                {
                    throw new InvalidOperationException($"Invalid message length {length}");
                }

                var frameSize = HeaderSize + length + CrcSize;
                if (_stream.Length - _stream.Position < frameSize)
                {
                    // Wait for more data;
                    break;
                }

                var payloadSpan = span.Slice(HeaderSize, length);
                var receivedCrc = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(HeaderSize + length, CrcSize));
                Span<byte> crcData = stackalloc byte[HeaderSize + length];
                span[..HeaderSize].CopyTo(crcData);
                payloadSpan.CopyTo(crcData[HeaderSize..]);
                var computedCrc = Crc32.HashToUInt32(crcData);
                if (computedCrc != receivedCrc)
                {
                    throw new InvalidOperationException("CRC mismatch in message");
                }

                var payload = new byte[length];
                payloadSpan.CopyTo(payload);

                _onMessage(MessageWrapper.From((MessageType)type, payload));

                _stream.Position += frameSize;
            }

            // Compact remaining bytes
            if (_stream.Position > 0)
            {
                var remaining = _stream.Length - _stream.Position;

                if (remaining > 0)
                {
                    Array.Copy(
                        _stream.GetBuffer(),
                        _stream.Position,
                        _stream.GetBuffer(),
                        0,
                        remaining);
                }

                _stream.SetLength(remaining);
                _stream.Position = 0;
            }
        }
    }
}