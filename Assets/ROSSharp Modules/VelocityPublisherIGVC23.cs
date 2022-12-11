using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Autonav
{
    public class VelocityPublisherIGVC23 : UnityPublisher<MotorFeedback>
    {
        private MotorFeedback message;
        private SimpleCarController c;
        public Rigidbody rb;

        public float velocityNoiseStdDev = 0.05f;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new MotorFeedback();

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
            float delta_t = Time.fixedDeltaTime;
            // message.leftVel = c.vl + getRandNormal(0, velocityNoiseStdDev);
            // message.rightVel = c.vr + getRandNormal(0, velocityNoiseStdDev);

            message.delta_theta = (float)((c.vr - c.vl) * 0.1016 / 0.4826) * delta_t;
            message.delta_x = (float)((c.vl + c.vr) / 2.0 * Mathf.Cos(message.delta_theta)) * delta_t;
            message.delta_y = (float)((c.vl + c.vr) / 2.0 * Mathf.Sin(message.delta_theta)) * delta_t;

            Publish(message);
        }
    }
}