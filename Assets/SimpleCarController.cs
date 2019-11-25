using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCarController : MonoBehaviour
{

    public float speedMod = 1f;
    public float turnMod = 1f;
    public float wheelRadius = 0.127f;
    public float L = 0.6096f;
    public float Q = 0.3456f;

    public float vl = 0;
    public float vr = 0;

    private Rigidbody rb;

    public void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {

        float psi = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        float left = Mathf.Pow(Input.GetAxis("Vertical"),3);
        float right = -Mathf.Pow(Input.GetAxis("Vertical2"),3);

        Debug.Log("vl: " + vl + ", vr: " + vr);

        float psi_dot = (vl - vr) / L;

        vl = (float)System.Math.Tanh(left + vl);
        vr = (float)System.Math.Tanh(right + vr);

        if (vr - vl != 0)
        {

            Debug.Log("psidot: " + psi_dot);

            float dot_x = (vr + (vl - vr) / 4.0f) * Mathf.Sin(psi);
            float dot_y = (vr + (vl - vr) / 4.0f) * Mathf.Cos(psi);

            transform.Translate(new Vector3(dot_x, 0, dot_y) * Time.deltaTime * speedMod, Space.World);

            transform.Rotate(new Vector3(0, psi_dot * Mathf.Rad2Deg, 0) * Time.deltaTime * turnMod, Space.World);
            return;
        }

        float dx2 = wheelRadius / 2.0f * (vl + vr) * Mathf.Cos(psi);
        float dy2 = wheelRadius / 2.0f * (vl + vr) * Mathf.Sin(psi);

        Debug.Log("dx: " + dx2 + ", dy: " + dy2);

        // transform.Translate(new Vector3(-dy2, 0, dx2) * speedMod, Space.World);
        // transform.Rotate(new Vector3(0, 0, 0) * Time.deltaTime * turnMod);
    }
}