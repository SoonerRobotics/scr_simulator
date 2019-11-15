using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class AccelerationPublisher : Publisher<Messages.Geometry.Vector3>
    {

        private Messages.Geometry.Vector3 message;
        public Rigidbody rb;
        
        private Vector3 lastVelocity = Vector3.positiveInfinity;

        void Start()
        {
            base.Start();
            message = new Messages.Geometry.Vector3();
        }

        void FixedUpdate()
        {
            if (lastVelocity == Vector3.positiveInfinity)
            {
                lastVelocity = rb.velocity;
                return;
            }

            Vector3 accel = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = rb.velocity;

            message.x = accel.x;
            message.y = accel.y;
            message.z = accel.z;

            Publish(message);
        }
    }

}