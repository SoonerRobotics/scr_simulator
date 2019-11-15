using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class GPSPublisher : Publisher<Messages.Geometry.Vector3>
    {
        private Messages.Geometry.Vector3 message;
        public Transform tf;

        void Start()
        {
            base.Start();
            message = new Messages.Geometry.Vector3();
        }

        void FixedUpdate()
        {
            Vector3 pos = tf.position;
            message.x = pos.x / 78710.0f + 35.194881f;
            message.y = pos.z / 10247.0f + -92.438621f;
            Publish(message);
        }
    }
}