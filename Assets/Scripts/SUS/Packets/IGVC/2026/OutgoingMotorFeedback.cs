using System.IO;

namespace SUS.Packets.IGVC._2026
{
    public class OutgoingMotorFeedback : IOutgoingPacket
    {
        public readonly double DeltaX;
        public readonly double DeltaY;
        public readonly double DeltaTheta;

        public OutgoingMotorFeedback(double deltaX, double deltaY, double deltaTheta)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
            DeltaTheta = deltaTheta;
        }
        
        public void Write(BinaryWriter writer)
        {
            writer.Write(DeltaX);
            writer.Write(DeltaY);
            writer.Write(DeltaTheta);
        }
    }
}