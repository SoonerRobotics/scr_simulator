using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class VelocityPublisher : Publisher<Messages.Standard.Float64>
    {
        private Messages.Standard.Float64 message;
        public Rigidbody rb;

        protected override void Start()
        {
            base.Start();
            message = new Messages.Standard.Float64();
        }

        void FixedUpdate()
        {
            message.data = rb.velocity.magnitude;
            Publish(message);
        }
    }
}