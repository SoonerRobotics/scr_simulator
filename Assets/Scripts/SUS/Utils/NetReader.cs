using System;
using System.IO;

namespace SUS.Utils
{
    public static class NetReader
    {
        public static int ReadInt32BE(this BinaryReader r)
        {
            byte[] bytes = r.ReadBytes(4);
            if (bytes.Length < 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static long ReadInt64BE(this BinaryReader r)
        {
            byte[] bytes = r.ReadBytes(8);
            if (bytes.Length < 8)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static float ReadFloatBE(this BinaryReader r)
        {
            byte[] bytes = r.ReadBytes(4);
            if (bytes.Length < 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToSingle(bytes, 0);
        }

        public static double ReadDoubleBE(this BinaryReader r)
        {
            byte[] bytes = r.ReadBytes(8);
            if (bytes.Length < 8)
                throw new EndOfStreamException();
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}