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
        public float axleLength = 0.6096f;
        public float minSpeed = 0.1f;
        public float drag = 0.15f;

        public float vl, vr = 0; // angular velocities (radians per second)

        public float left, right = 0;

        public float forwardControl = 0;
        public float angularControl = 0;

        public bool useController = false;

        public bool useAngular = true;

        public Vector3 linear_vel { get; private set; } = new Vector3();
        public Vector3 angular_vel { get; private set; } = new Vector3();
        public Vector3 accel { get; private set; } = new Vector3();

        public void Start()
        {
            useAngular = ConfigLoader.Instance.control.motors.useAngularVelocity;
            drag = ConfigLoader.Instance.control.motors.velocityDecay;
            speedMod = ConfigLoader.Instance.control.manual.fullSpeed;
        }

        public void FixedUpdate()
        {

            float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            if (useController)
            {
                float horiz = Mathf.Pow(Input.GetAxis("Vertical"), 3);
                float vertical = Input.GetAxis("Horizontal");
                left = (horiz * vertical + horiz) * speedMod / wheelRadius;
                right = (-horiz * vertical + horiz) * speedMod / wheelRadius;
            }
            else
            {
                left = (forwardControl / wheelRadius) - ((0.3f) * (angularControl / wheelRadius));
                right = (forwardControl / wheelRadius) + ((0.3f) * (angularControl / wheelRadius));
            }

            float psi_dot = wheelRadius * (vl - vr) / axleLength;

            if (Mathf.Abs(drag * left) < minSpeed) {
                left = 0;
            }

            if (Mathf.Abs(drag * right) < minSpeed) {
                right = 0;
            }

            vl = drag * left + (1.0f - drag) * vl;
            vr = drag * right + (1.0f - drag) * vr;

            // Debug.Log("Speed out: " + vl + ", " + vr);

            float dot_x = wheelRadius / 2.0f * (vl + vr) * Mathf.Sin(psi);
            float dot_y = wheelRadius / 2.0f * (vl + vr) * Mathf.Cos(psi);

            Vector3 new_linear_vel = new Vector3(dot_x, 0, dot_y);

            accel = (new_linear_vel - linear_vel) / Time.fixedDeltaTime;

            linear_vel = new_linear_vel;

            transform.Translate(linear_vel * Time.fixedDeltaTime, Space.World);

            angular_vel = new Vector3(0, psi_dot, 0);

            transform.Rotate(angular_vel * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.World);

        }
    }
}