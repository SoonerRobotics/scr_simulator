using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class VelocityPublisher : UnityPublisher<Velocity>
    {
        private Velocity message;
        private SimpleCarController c;
        public Rigidbody rb;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Velocity();
        }

        void FixedUpdate()
        {
            message.leftVel = c.vl;
            message.rightVel = c.vr;
            Publish(message);
        }
    }
}