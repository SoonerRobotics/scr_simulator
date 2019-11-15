using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public Transform leftWheelVisual;
    public Transform rightWheelVisual;
    public bool motor;
    public bool freeSpinning;
}

public class SimpleCarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    private Rigidbody rb;

    public void Start()
    {
        this.rb = this.GetComponent<Rigidbody>();
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider, Transform visualWheel)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation * Quaternion.Euler(0, 0, 90);
    }

    public void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.freeSpinning)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;

                axleInfo.leftWheel.motorTorque = 0.0001f;
                axleInfo.rightWheel.motorTorque = -0.0001f;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = maxMotorTorque * Input.GetAxis("Vertical");
                axleInfo.rightWheel.motorTorque = -maxMotorTorque * Input.GetAxis("Vertical2");
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel, axleInfo.leftWheelVisual);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel, axleInfo.rightWheelVisual);
            // Debug.Log(this.rb.velocity.magnitude * 2.237 + "mph");
        }
    }
}