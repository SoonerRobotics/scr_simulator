using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Nrc
{
    public class DriveStatusPublisher : UnityPublisher<DriveStatus>
    {
        private DriveStatus message;
        public Transform tf;

        private SimpleCarController c;
        private float lastVelocity = float.PositiveInfinity;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new DriveStatus();
            message.device_id = 1;
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

            message.yaw = tf.rotation.eulerAngles.y;
            message.acceleration = accel;
            message.left_speed = c.vl;
            message.right_speed = c.vr;

            Publish(message);
        }
    }
}