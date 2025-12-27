using System.IO;
using SUS.Utils;

namespace SUS.Packets.IGVC._2026
{
    public class IncomingMotorInput : IncomingPacket
    {
        public float ForwardVelocity;
        public float SidewaysVelocity;
        public float AngularVelocity;

        public static IncomingMotorInput Read(BinaryReader reader)
        {
            return new IncomingMotorInput()
            {
                ForwardVelocity = (float)reader.ReadDoubleBE(),
                SidewaysVelocity = (float)reader.ReadDoubleBE(),
                AngularVelocity = (float)reader.ReadDoubleBE()
            };
        }
    }
}