using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class AccelerationPublisher : Publisher<Messages.Standard.Float64>
    {

        private Messages.Standard.Float64 message;
        public Rigidbody rb;
        
        private Vector3 lastVelocity = Vector3.positiveInfinity;

        protected override void Start()
        {
            base.Start();
            message = new Messages.Standard.Float64();
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

            message.data = accel.magnitude;

            Publish(message);
        }
    }

}