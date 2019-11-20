using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleCarController : MonoBehaviour
{

    public float speedMod = 1f;
    public float turnMod = 100f;
    public float wheelRadius = 0.127f;
    public float L = 0.6096f;

    float vl = 0;
    float vr = 0;

    private Rigidbody rb;

    public void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {

        float x = transform.position.z;
        float y = -transform.position.x;
        float psi = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;


        float left = Mathf.Pow(Input.GetAxis("Vertical"),3);
        float right = Mathf.Pow(Input.GetAxis("Vertical2"),3);

        Debug.Log("vl: " + vl + ", vr: " + vr);

        float psi_dot = (vl + vr) / L;

        vl = (float)System.Math.Tanh(left + vl);
        vr = (float)System.Math.Tanh(right + vr);

        if (vr - vl != 0)
        {

            float R = L * (vl + vr) / (2 * (vr - vl));
            float ICC_x = x - R * Mathf.Sin(psi);
            float ICC_y = y + R * Mathf.Cos(psi);

            Debug.Log("psidot: " + psi_dot);

            if (R < 100)
            {
                float new_x = ((x - ICC_x) * Mathf.Cos(psi_dot * Time.deltaTime) - (y - ICC_y) * Mathf.Sin(psi_dot * Time.deltaTime) + ICC_x);
                float new_y = ((x - ICC_x) * Mathf.Sin(psi_dot * Time.deltaTime) + (y - ICC_y) * Mathf.Cos(psi_dot * Time.deltaTime) + ICC_y);
                float dx = new_x - x;
                float dy = new_y - y;

                transform.Translate(new Vector3(-dy, 0, dx) * speedMod, Space.World);
                transform.Rotate(new Vector3(0, -psi_dot * Mathf.Rad2Deg, 0) * Time.deltaTime * turnMod);

                return;
            }
        }

        float dx2 = wheelRadius / 2.0f * (vl + vr) * Mathf.Cos(psi);
        float dy2 = wheelRadius / 2.0f * (vl + vr) * Mathf.Sin(psi);

        Debug.Log("dx: " + dx2 + ", dy: " + dy2);

        transform.Translate(new Vector3(-dy2, 0, dx2) * speedMod, Space.World);
        // transform.Rotate(new Vector3(0, 0, 0) * Time.deltaTime * turnMod);
    }
}