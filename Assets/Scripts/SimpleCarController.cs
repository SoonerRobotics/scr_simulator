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

        public float vl = 0;
        public float vr = 0;

        public float leftControl = 0;
        public float rightControl = 0;

        public bool useController = false;

        public void Start()
        {
        }

        public void FixedUpdate()
        {

            float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            float left;
            float right;
            if (useController)
            {
                left = Mathf.Pow(Input.GetAxis("Vertical"), 3);
                right = -Mathf.Pow(Input.GetAxis("Vertical2"), 3);
            }
            else
            {
                left = leftControl;
                right = rightControl;
            }

            float psi_dot = (vl - vr) / L;

            vl = (1.0f - drag) * left + drag * vl;
            vr = (1.0f - drag) * right + drag * vr;

            float dot_x = (vr + (vl - vr) / 4.0f) * Mathf.Sin(psi);
            float dot_y = (vr + (vl - vr) / 4.0f) * Mathf.Cos(psi);

            transform.Translate(new Vector3(dot_x, 0, dot_y) * Time.deltaTime * speedMod, Space.World);

            transform.Rotate(new Vector3(0, psi_dot * Mathf.Rad2Deg, 0) * Time.deltaTime * turnMod, Space.World);

        }
    }
}