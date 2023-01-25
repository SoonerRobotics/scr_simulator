using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Sensor
{
    public class IMUPublisher : UnityPublisher<Imu>
    {
        private Imu message;

        public float accelNoiseStdDev = 0.15f;
        public float orientationNoiseStdDev = 0.017f;
        public float angularVelocityNoiseStdDev = 0.017f;

        private SimpleCarController c;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Imu();

            accelNoiseStdDev = ConfigLoader.Instance.sensors.imu.accelNoise;
            orientationNoiseStdDev = ConfigLoader.Instance.sensors.imu.orientationNoise;
            angularVelocityNoiseStdDev = ConfigLoader.Instance.sensors.imu.angularVelocityNoise;
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

            Quaternion orientation = c.transform.rotation;

            // Sprinkle in that sick noise
            orientation = Quaternion.Euler(orientation.eulerAngles + new Vector3(getRandNormal(0, orientationNoiseStdDev), getRandNormal(0, orientationNoiseStdDev), getRandNormal(0, orientationNoiseStdDev)));

            message.orientation.w = orientation.w;
            message.orientation.x = orientation.x;
            message.orientation.y = orientation.y;
            message.orientation.z = orientation.z;

            Vector3 accel = c.transform.worldToLocalMatrix * c.accel; // sick maths

            message.linear_acceleration.x = accel.z + getRandNormal(0, accelNoiseStdDev);
            message.linear_acceleration.y = accel.x + getRandNormal(0, accelNoiseStdDev);
            message.linear_acceleration.z = accel.y + getRandNormal(0, accelNoiseStdDev);

            message.angular_velocity.x = c.angular_vel.z + getRandNormal(0, angularVelocityNoiseStdDev);
            message.angular_velocity.y = -c.angular_vel.x + getRandNormal(0, angularVelocityNoiseStdDev);
            message.angular_velocity.z = -c.angular_vel.y + getRandNormal(0, angularVelocityNoiseStdDev);

            Publish(message);
        }
    }
}