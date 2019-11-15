using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class VelocityPublisher : Publisher<Messages.Geometry.Vector3>
    {
        private Messages.Geometry.Vector3 message;
        public Rigidbody rb;

        void Start()
        {
            base.Start();
            message = new Messages.Geometry.Vector3();
        }

        void FixedUpdate()
        {
            Vector3 vel = rb.velocity;
            message.x = vel.x;
            message.y = vel.y;
            message.z = vel.z;
            Publish(message);
        }
    }
}