using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class MotorsSubscriber : Subscriber<Messages.IGVC.Motors>
    {
        public float left;
        public float right;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(Messages.IGVC.Motors motors)
        {
            left = motors.left;
            right = motors.right;
        }
    }
}

