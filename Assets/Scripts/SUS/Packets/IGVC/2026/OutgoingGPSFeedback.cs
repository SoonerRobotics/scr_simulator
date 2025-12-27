using System.IO;

namespace SUS.Packets.IGVC._2026
{
    public class OutgoingGPSFeedback : IOutgoingPacket
    {
        public readonly double Latitude;
        public readonly double Longitude;

        public OutgoingGPSFeedback(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        
        public void Write(BinaryWriter writer)
        {
            writer.Write(Latitude);
            writer.Write(Longitude);
        }
    }
}