using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class IGVCMotorsPublisher : UnityPublisher<Motors>
    {
        private SimpleCarController car;
        public Motors message;

        protected override void Start()
        {
            base.Start();
            message = new Motors();
            car = GetComponent<SimpleCarController>();
        }

        void FixedUpdate()
        {
            message.left = car.left;
            message.right = car.right;
            Publish(message);
        }
    }
}

