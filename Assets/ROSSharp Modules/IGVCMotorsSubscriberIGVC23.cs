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
            car.forwardControl = motors.forward_velocity;
            car.angularControl = motors.angular_velocity;
        }
    }
}

