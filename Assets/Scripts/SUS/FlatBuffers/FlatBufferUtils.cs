using System;
using System.IO;
using Google.FlatBuffers;
using SUS.FlatBuffers.arc;

namespace SUS.FlatBuffers
{
    public static class FlatBufferUtils
    {
        public enum FlatBufferType : byte
        {
            ImageFrame = 0x01,
            ArcLOG = 0x02
        }

        public static FlatBufferType? FromByte(byte b)
        {
            return Enum.IsDefined(typeof(FlatBufferType), b)
                ? (FlatBufferType)b
                : null;
        }

        // =========================
        // FlatBufferWrapper
        // =========================
        public class FlatBufferWrapper
        {
            public FlatBufferType MessageType { get; }
            private int PayloadLength { get; }
            public byte[] Payload { get; }

            private FlatBufferWrapper(
                FlatBufferType messageType,
                int payloadLength,
                byte[] payload)
            {
                if (payload.Length != payloadLength)
                    throw new ArgumentException("Payload length does not match actual payload size");

                MessageType = messageType;
                PayloadLength = payloadLength;
                Payload = payload;
            }

            /// <summary>
            /// Read FlatBufferWrapper from stream
            /// </summary>
            public static FlatBufferWrapper FromStream(BinaryReader reader)
            {
                try
                {
                    byte typeByte = reader.ReadByte();
                    var type = FromByte(typeByte);
                    if (type == null)
                        return null;
                    
                    var payloadLength = ReadInt32BigEndian(reader);
                    var payload = reader.ReadBytes(payloadLength);
                    return payload.Length != payloadLength ? null : new FlatBufferWrapper(type.Value, payloadLength, payload);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            public static FlatBufferWrapper Create(
                FlatBufferType type,
                byte[] payload)
            {
                return new FlatBufferWrapper(type, payload.Length, payload);
            }

            /// <summary>
            /// Serialize to wire format
            /// | type | length (4) | payload |
            /// </summary>
            public byte[] ToByteArray(bool bigEndian = true)
            {
                var totalLength = 1 + 4 + PayloadLength;
                var buffer = new byte[totalLength];

                var index = 0;
                buffer[index++] = (byte)MessageType;

                if (bigEndian)
                {
                    buffer[index++] = (byte)((PayloadLength >> 24) & 0xFF);
                    buffer[index++] = (byte)((PayloadLength >> 16) & 0xFF);
                    buffer[index++] = (byte)((PayloadLength >> 8) & 0xFF);
                    buffer[index++] = (byte)(PayloadLength & 0xFF);
                }
                else
                {
                    buffer[index++] = (byte)(PayloadLength & 0xFF);
                    buffer[index++] = (byte)((PayloadLength >> 8) & 0xFF);
                    buffer[index++] = (byte)((PayloadLength >> 16) & 0xFF);
                    buffer[index++] = (byte)((PayloadLength >> 24) & 0xFF);
                }

                Buffer.BlockCopy(Payload, 0, buffer, index, PayloadLength);
                return buffer;
            }

            private static int ReadInt32BigEndian(BinaryReader reader)
            {
                var bytes = reader.ReadBytes(4);
                if (bytes.Length < 4)
                    throw new EndOfStreamException();

                return (bytes[0] << 24)
                     | (bytes[1] << 16)
                     | (bytes[2] << 8)
                     | bytes[3];
            }
        }
        
        public static class FlatBufferConverter
        {
            private static void CheckType(
                FlatBufferWrapper wrapper,
                FlatBufferType expected)
            {
                if (wrapper.MessageType != expected)
                {
                    throw new ArgumentException(
                        $"FlatBufferWrapper is not of type {expected}"
                    );
                }
            }

            public static ImageFrame AsImageFrame(FlatBufferWrapper wrapper)
            {
                CheckType(wrapper, FlatBufferType.ImageFrame);
                return ImageFrame.GetRootAsImageFrame(
                    new ByteBuffer(wrapper.Payload)
                );
            }

            public static ArcLog AsArcLog(FlatBufferWrapper wrapper)
            {
                CheckType(wrapper, FlatBufferType.ArcLOG);
                return ArcLog.GetRootAsArcLog(
                    new ByteBuffer(wrapper.Payload)
                );
            }
        }
    }
}
