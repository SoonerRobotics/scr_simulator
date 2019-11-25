using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class GPSPublisher : Publisher<Messages.IGVC.GPS>
    {
        private Messages.IGVC.GPS message;
        public Transform tf;

        protected override void Start()
        {
            base.Start();
            message = new Messages.IGVC.GPS();
        }

        void FixedUpdate()
        {
            Vector3 pos = tf.position;
            message.latitude = pos.z / 78710.0f + 35.194881f;
            message.longitude = -pos.x / 10247.0f + -97.438621f;
            Publish(message);
        }
    }
}