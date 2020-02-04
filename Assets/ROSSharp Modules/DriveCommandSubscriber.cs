using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Nrc
{
    public class DriveCommandSubscriber : UnitySubscriber<DriveCommand>
    {
        private SimpleCarController car;

        protected override void Start()
        {
            base.Start();
            car = GetComponent<SimpleCarController>();
        }

        protected override void ReceiveMessage(DriveCommand motors)
        {
            //car.leftControl = motors.left;
            //car.rightControl = motors.right;
        }
    }
}
