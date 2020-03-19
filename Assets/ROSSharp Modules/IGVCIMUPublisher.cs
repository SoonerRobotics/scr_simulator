using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class IGVCIMUPublisher : UnityPublisher<Imuodom>
    {
        private Imuodom message;
        public Transform tf;

        public float accelNoiseStdDev = 0.15f;
        public float headingNoiseStdDev = 0.017f;

        public bool ccw = true;
        public bool radians = true;

        private SimpleCarController c;
        private float lastVelocity = float.PositiveInfinity;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Imuodom();

            accelNoiseStdDev = ConfigLoader.Instance.sensors.imu.accelNoise;
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
                lastVelocity = (c.vr + c.vl) / c.axleLength;
                return;
            }

            float accel = ((c.vr + c.vl) / c.axleLength - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = (c.vr + c.vl) / c.axleLength;

            message.heading = tf.rotation.eulerAngles.y;
            if (ccw)
            {
                message.heading = 360 - message.heading;
            }
            if (radians)
            {
                message.heading *= Mathf.Deg2Rad;
            }
            message.heading += getRandNormal(0, headingNoiseStdDev);
            message.acceleration = accel + getRandNormal(0, accelNoiseStdDev);
            Publish(message);
        }
    }
}