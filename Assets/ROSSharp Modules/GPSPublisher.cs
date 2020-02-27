using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class GPSPublisher : UnityPublisher<Gps>
    {
        private Gps message;
        public Transform tf;

        public float latNoiseStdDev = 1.843f;
        public float lonNoiseStdDev = 2.138f;

        public float lat0Pos = 35.194881f;
        public float lon0Pos = -97.438621f;

        protected override void Start()
        {
            base.Start();
            message = new Gps();
        }

        public float getRandNormal(float mean, float stdDev)
        {
            float u1 = 1.0f - Random.value; //uniform(0,1] random doubles
            float u2 = 1.0f - Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                         Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)

            return stdDev * randStdNormal;
        }

        void FixedUpdate()
        {
            Vector3 pos = tf.position;
            message.latitude = (pos.z + getRandNormal(0, latNoiseStdDev)) / 110944.12 + lat0Pos;
            message.longitude = (pos.x + getRandNormal(0, lonNoiseStdDev)) / 91071.17 + lon0Pos;
            Publish(message);
        }
    }
}