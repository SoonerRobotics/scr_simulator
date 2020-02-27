using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeedometerScript : MonoBehaviour
{

    public TextMeshProUGUI textMilesPerHour;
    public TextMeshProUGUI textMetersPerSecond;
    public GameObject robot;

    private float curSpeed = 0.0f;
    private Vector3 lastPos = new Vector3(0, 0, 0);

    public void InitSpeedometer()
    {
        this.robot = LevelInitalizer.robot;
        lastPos = this.robot.transform.position;
    }

    private void Update()
    {
        if (robot)
        {
            textMetersPerSecond.text = $"{curSpeed:0.0} m/s";
            textMilesPerHour.text = $"{curSpeed * 2.237:0.0} mph"; ;
        }
    }

    void FixedUpdate()
    {
        if (robot)
        {
            float instSpeed = (robot.transform.position - lastPos).magnitude / Time.fixedDeltaTime;

            curSpeed = 0.85f * instSpeed + 0.15f * curSpeed;

            lastPos = robot.transform.position;
        }
    }
}
