using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class IGVCMotorsSubscriber : UnitySubscriber<Motors>
    {
        private SimpleCarController car;

        protected override void Start()
        {
            base.Start();
            car = GetComponent<SimpleCarController>();
        }

        protected override void ReceiveMessage(Motors motors)
        {
            car.leftControl = motors.left;
            car.rightControl = motors.right;
        }
    }
}

