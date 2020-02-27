using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class VelocityPublisher : UnityPublisher<Velocity>
    {
        private Velocity message;
        private SimpleCarController c;
        public Rigidbody rb;

        public float velocityNoiseStdDev = 0.05f;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Velocity();
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
            message.leftVel = c.vl + getRandNormal(0, velocityNoiseStdDev);
            message.rightVel = c.vr + getRandNormal(0, velocityNoiseStdDev);
            Publish(message);
        }
    }
}