using System.IO;

namespace SUS.Packets
{
    public interface IOutgoingPacket
    {
        void Write(BinaryWriter writer);
    }
}