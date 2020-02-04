using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Nrc
{
    public class DriveCommandSubscriber : UnitySubscriber<DriveCommand>
    {
        private SimpleCarController car;

        private DriveStatusPublisher pubby;

        protected override void Start()
        {
            base.Start();
            car = GetComponent<SimpleCarController>();
            pubby = GetComponent<DriveStatusPublisher>();
        }

        protected override void ReceiveMessage(DriveCommand motors)
        {
            float curHeading = pubby.message.yaw;

            float delta = motors.heading - curHeading;
            delta = (delta + 180) % 360 - 180;

            car.leftControl = motors.speed + (delta / 180);
            car.rightControl = motors.speed - (delta / 180);

        }
    }
}
