using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient.MessageTypes.Igvc;

namespace RosSharp.RosBridgeClient
{
    public class SimpleCarController : MonoBehaviour
    {

        public float speedMod = 1f;
        public float turnMod = 1f;
        public float wheelRadius = 0.127f;
        public float L = 0.6096f;
        public float drag = 0.85f;

        public float vl = 0; // angular velocities
        public float vr = 0; // (radians per second)

        public float leftControl = 0;
        public float rightControl = 0;

        public bool useController = false;

        public void Start()
        {
        }

        public void FixedUpdate()
        {

            float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            float left, right;

            if (useController)
            {
                float horiz = Mathf.Pow(Input.GetAxis("Vertical"), 3);
                float vertical = Input.GetAxis("Horizontal");
                left = (horiz * vertical + horiz) * speedMod / wheelRadius;
                right = (-horiz * vertical + horiz) * speedMod / wheelRadius;
            }
            else
            {
                left = leftControl / wheelRadius;
                right = rightControl / wheelRadius;
            }

            float psi_dot = wheelRadius * (vl - vr) / L;

            vl = (1.0f - drag) * left + drag * vl;
            vr = (1.0f - drag) * right + drag * vr;

            float dot_x = wheelRadius / 2.0f * (vl + vr) * Mathf.Sin(psi);
            float dot_y = wheelRadius / 2.0f * (vl + vr) * Mathf.Cos(psi);

            transform.Translate(new Vector3(dot_x, 0, dot_y) * Time.fixedDeltaTime, Space.World);

            transform.Rotate(new Vector3(0, psi_dot * Mathf.Rad2Deg, 0) * Time.fixedDeltaTime, Space.World);

        }
    }
}