using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;
using UnityEngine.Serialization;

namespace Robots
{
    public class VectornavModule : MonoBehaviour
    {
        public double interval = 0.25;
        public double originLatitude = 35.194881;
        public double originLongitude = -97.438621;
        public double latitudeLength = 110944.12;
        public double longitudeLength = 91071.17;

        private double _nextSend;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            // Check if it is time to send a new gps
            if (Time.time < _nextSend)
            {
                return;
            }

            // Determine YPR
            var ypr = transform.rotation.eulerAngles;
        
            _nextSend = Time.time + interval;
            var builder = new FlatBufferBuilder(1024);
            var imageOffset = VectornavReport.CreateVectornavReport(
                builder,
                (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                0,
                transform.position.z / latitudeLength + originLatitude,
                transform.position.x / longitudeLength + originLongitude,
                ypr.x, // Pitch
                ypr.z, // Roll
                ypr.y, // Yaw
                0,
                0,
                0,
                7,
                3
            );
            builder.Finish(imageOffset.Value);

            var wrapper = MessageWrapper.From(MessageType.VectorNav, builder.SizedByteArray());
            SusConnection.Instance.Broadcast(wrapper);
        }
    }
}
