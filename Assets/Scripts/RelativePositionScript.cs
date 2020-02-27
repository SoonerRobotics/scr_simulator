using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RelativePositionScript : MonoBehaviour
{

    public TextMeshProUGUI textX;
    public TextMeshProUGUI textY;
    public GameObject robot;

    private Vector3 initPosition = new Vector3(0, 0, 0);
    private Vector3 curPosition = new Vector3(0, 0, 0);

    public void InitRelativePos()
    {
        this.robot = LevelInitalizer.robot;
        initPosition = this.robot.transform.position;
    }

    private void Update()
    {
        if (robot)
        {
            textX.text = $"x: {curPosition.z - initPosition.z:0.0}";
            textY.text = $"y: {initPosition.x - curPosition.x:0.0}";
        }
    }

    void FixedUpdate()
    {
        if (robot)
        {
            curPosition = this.robot.transform.position;
        }
    }
}
