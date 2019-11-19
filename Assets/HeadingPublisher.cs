using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class HeadingPublisher : Publisher<Messages.Standard.Float64>
    {
        private Messages.Standard.Float64 message;
        public Transform tf;

        protected override void Start()
        {
            base.Start();
            message = new Messages.Standard.Float64();
        }

        void FixedUpdate()
        {
            message.data = tf.rotation.eulerAngles.y;
            Publish(message);
        }
    }
}