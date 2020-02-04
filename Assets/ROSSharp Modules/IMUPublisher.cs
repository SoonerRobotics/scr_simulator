using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class IMUPublisher : UnityPublisher<Imuodom>
    {
        private Imuodom message;
        public Transform tf;

        private SimpleCarController c;
        private float lastVelocity = float.PositiveInfinity;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Imuodom();
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

            message.heading = tf.rotation.eulerAngles.y;
            message.acceleration = accel;
            Publish(message);
        }
    }
}