using System.IO;

namespace SUS.Utils
{
    public class NetworkBinaryReader : BinaryReader
    {
        public NetworkBinaryReader(Stream s) : base(s) {}

        public int ReadInt() => this.ReadInt32BE();
        public long ReadLong() => this.ReadInt64BE();
        public float ReadFloat() => this.ReadFloatBE();
        public double ReadDouble() => this.ReadDoubleBE();
    }

}