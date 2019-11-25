using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class MotorsSubscriber : Subscriber<Messages.motors>
    {
        public float left;
        public float right;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(Messages.motors motors)
        {
            left = motors.left;
            right = motors.right;
        }
    }
}

