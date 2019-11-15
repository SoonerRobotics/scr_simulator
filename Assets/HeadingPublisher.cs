using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class HeadingPublisher : Publisher<Messages.Standard.Float64>
    {
        private Messages.Standard.Float64 message;
        public Transform tf;

        void Start()
        {
            base.Start();
            message = new Messages.Standard.Float64();
        }

        void FixedUpdate()
        {
            message.data = Vector3.SignedAngle(Vector3.forward, tf.forward, Vector3.up);
            if (message.data < 0)
            {
                message.data = -(message.data - 360);
            }
            Publish(message);
        }
    }
}