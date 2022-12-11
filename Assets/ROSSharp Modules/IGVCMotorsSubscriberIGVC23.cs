using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Autonav
{
    public class IGVCMotorsSubscriberIGVC23 : UnitySubscriber<MotorInput>
    {
        private SimpleCarController car;

        protected override void Start()
        {
            base.Start();
            car = GetComponent<SimpleCarController>();
        }

        protected override void ReceiveMessage(MotorInput motors)
        {
            car.leftControl = motors.left_motor;
            car.rightControl = motors.right_motor;
        }
    }
}

