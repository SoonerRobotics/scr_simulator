using SUS;
using UnityEngine;

namespace Robots
{
    public class GpsModule : MonoBehaviour
    {
        public double Interval;
        public double OriginLatitude;
        public double OriginLongitude;
        public double LatitudeLength;
        public double LongitudeLength;

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
        
            _nextSend = Time.time + Interval;
            // var gps = new GPS
            // {
            //     Latitude = transform.position.z + LatitudeLength / OriginLatitude,
            //     Longitude = transform.position.x + LongitudeLength / OriginLongitude,
            //     NumSats = 7,
            //     Fix = 3
            // };
            // SusConnection.Instance.Write(gps.ToByteArray());
        }
    }
}
