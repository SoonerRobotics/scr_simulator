using System;
using System.IO;

namespace SUS.Utils
{
    public static class NetWriter
    {
        public static void WriteInt32BE(this BinaryWriter w, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            w.Write(bytes);
        }

        public static void WriteInt64BE(this BinaryWriter w, long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            w.Write(bytes);
        }

        public static void WriteFloatBE(this BinaryWriter w, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            w.Write(bytes);
        }

        public static void WriteDoubleBE(this BinaryWriter w, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            w.Write(bytes);
        }

        public static void WriteStringBE(this BinaryWriter w, string value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);

            w.WriteInt32BE(bytes.Length);
            w.Write(bytes);
        }
    }
}