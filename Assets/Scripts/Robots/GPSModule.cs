using SUS.Packets.IGVC._2026;
using UnityEngine;

namespace Robots
{
    public class GpsModule : MonoBehaviour
    {
        private SUSConnection _susConnection;

        public double Interval;
        public double OriginLatitude;
        public double OriginLongitude;
        public double LatitudeLength;
        public double LongitudeLength;

        private double _nextSend;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _susConnection = SUSConnection.GetOrCreateInstance();
        }

        // Update is called once per frame
        void Update()
        {
            // Check if it is time to send a new gps
            if (Time.time < _nextSend)
            {
                return;
            }
        
            _nextSend = Time.time + Interval;
            OutgoingGPSFeedback msg = new(
                transform.position.z + LatitudeLength / OriginLatitude,
                transform.position.x + LongitudeLength / OriginLongitude
            );
            _susConnection.Write(msg);
        }
    }
}
