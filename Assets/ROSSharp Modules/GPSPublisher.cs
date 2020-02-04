using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class GPSPublisher : UnityPublisher<Gps>
    {
        private Gps message;
        public Transform tf;

        private System.Random random;

        protected override void Start()
        {
            base.Start();
            random = new System.Random();
            message = new Gps();
        }

        public float getRandNormal(float mean, float stdDev)
        {
            float u1 = 1.0f - (float)random.NextDouble(); //uniform(0,1] random doubles
            float u2 = 1.0f - (float)random.NextDouble();
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                         Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)

            return stdDev * randStdNormal;
        }

        void FixedUpdate()
        {
            Vector3 pos = tf.position;
            message.latitude = (pos.z + getRandNormal(0, 1.843f)) / 110944.12 + 35.194881f;
            message.longitude = (pos.x + getRandNormal(0, 2.138f)) / 91071.17 + -97.438621f;
            Publish(message);
        }
    }
}