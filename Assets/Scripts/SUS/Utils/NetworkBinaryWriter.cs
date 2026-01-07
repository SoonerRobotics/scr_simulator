using System.IO;

namespace SUS.Utils
{
    public class NetworkBinaryWriter : BinaryWriter
    {
        public NetworkBinaryWriter(Stream s) : base(s) {}

        public void WriteByte(byte v) => base.Write(v);
        public void WriteInt(int v) => this.WriteInt32BE(v);
        public void WriteLong(long v) => this.WriteInt64BE(v);
        public void WriteFloat(float v) => this.WriteFloatBE(v);
        public void WriteDouble(double v) => this.WriteDoubleBE(v);
        public void WriteString(string v) => this.WriteStringBE(v);
    }
}