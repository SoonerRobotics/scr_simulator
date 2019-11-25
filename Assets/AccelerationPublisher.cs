using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class AccelerationPublisher : Publisher<Messages.Standard.Float64>
    {

        private Messages.Standard.Float64 message;
        private SimpleCarController c;
        public Rigidbody rb;
        
        private float lastVelocity = float.PositiveInfinity;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<SimpleCarController>();
            message = new Messages.Standard.Float64();
        }

        void FixedUpdate()
        {
            if (lastVelocity == float.PositiveInfinity)
            {
                lastVelocity = (c.vr + c.vl) / c.L;
                return;
            }

            float accel = ((c.vr + c.vl) / c.L - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = (c.vr + c.vl) / c.L;

            message.data = accel;

            Debug.Log("accel: " + accel);

            Publish(message);
        }
    }

}