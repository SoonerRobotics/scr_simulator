using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class VelocityPublisher : Publisher<Messages.IGVC.Velocity>
    {
        private Messages.IGVC.Velocity message;
        private SimpleCarController c;
        public Rigidbody rb;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Messages.IGVC.Velocity();
        }

        void FixedUpdate()
        {
            message.leftVel = c.vl;
            message.rightVel = c.vr;
            Publish(message);
        }
    }
}