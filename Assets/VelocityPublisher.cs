using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class VelocityPublisher : Publisher<Messages.Standard.Float64>
    {
        private Messages.Standard.Float64 message;
        private SimpleCarController c;
        public Rigidbody rb;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Messages.Standard.Float64();
        }

        void FixedUpdate()
        {
            message.data = (c.vr + c.vl) / c.L;
            Debug.Log("velocity: " + message.data);
            Publish(message);
        }
    }
}