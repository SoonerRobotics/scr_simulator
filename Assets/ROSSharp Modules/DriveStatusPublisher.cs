using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Nrc
{
    public class DriveStatusPublisher : UnityPublisher<DriveStatus>
    {
        public DriveStatus message;
        public Transform tf;

        private SimpleCarController c;
        private float lastVelocity = float.PositiveInfinity;

        public float accelNoiseStdDev = 0.15f;
        public float headingNoiseStdDev = 1f;

        public float velocityNoiseStdDev = 0.05f;

        public bool ccw = true;
        public bool radians = true;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new DriveStatus();
            message.device_id = 1;

            accelNoiseStdDev = ConfigLoader.Instance.sensors.imu.accelNoise;
            headingNoiseStdDev = ConfigLoader.Instance.sensors.imu.headingNoise;
            ccw = ConfigLoader.Instance.sensors.imu.headingCCW;
            radians = ConfigLoader.Instance.sensors.imu.headingRadians;
            velocityNoiseStdDev = ConfigLoader.Instance.sensors.encoders.velocityNoise;
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

            message.yaw = tf.rotation.eulerAngles.y;
            if (ccw)
            {
                message.yaw = 360 - message.yaw;
            }
            if (radians)
            {
                message.yaw *= Mathf.Deg2Rad;
            }
            message.yaw += getRandNormal(0, headingNoiseStdDev);
            message.acceleration = accel + getRandNormal(0, accelNoiseStdDev);
            message.left_speed = c.vl + getRandNormal(0, velocityNoiseStdDev);
            message.right_speed = c.vr + getRandNormal(0, velocityNoiseStdDev);

            Publish(message);
        }
    }
}