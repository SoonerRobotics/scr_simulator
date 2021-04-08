using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompassScript : MonoBehaviour
{

    public TextMeshProUGUI textDegrees;
    public TextMeshProUGUI textRadians;
    public GameObject robot;

    private static string[] caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

    public void InitCompass()
    {
        this.robot = LevelInitalizer.robot;
    }

    // Via https://gist.github.com/adrianstevens/8163205
    public static string DegreesToCardinal(double degrees)
    {
        return caridnals[ (int)Math.Round(((double)degrees % 360) / 45) ];
    }

    // Update is called once per frame
    void Update()
    {
        if (robot)
        {
            float heading = robot.transform.rotation.eulerAngles.y;
            textDegrees.text = $"{heading:0}° {DegreesToCardinal(heading)}";
            float adjHeading = 360 - heading;
            textRadians.text = $"{adjHeading / 180.0 * Math.PI:0.0}";
        }
    }
}
