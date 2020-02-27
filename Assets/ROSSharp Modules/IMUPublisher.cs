using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class IMUPublisher : UnityPublisher<Imuodom>
    {
        private Imuodom message;
        public Transform tf;

        public float accelNoiseStdDev = 0.15f;
        public float headingNoiseStdDev = 1f;

        private SimpleCarController c;
        private float lastVelocity = float.PositiveInfinity;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Imuodom();
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

            if (lastVelocity == float.PositiveInfinity)
            {
                lastVelocity = (c.vr + c.vl) / c.L;
                return;
            }

            float accel = ((c.vr + c.vl) / c.L - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = (c.vr + c.vl) / c.L;

            message.heading = tf.rotation.eulerAngles.y + getRandNormal(0, headingNoiseStdDev);
            message.acceleration = accel + getRandNormal(0, accelNoiseStdDev);
            Publish(message);
        }
    }
}