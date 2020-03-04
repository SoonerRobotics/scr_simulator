using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompassScript : MonoBehaviour
{

    public TextMeshProUGUI textMeshProUgui;
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

    private void Start()
    {
        caridnals = ConfigLoader.Instance.configExample.caridnals;
    }

    // Update is called once per frame
    void Update()
    {
        if (robot)
        {
            float heading = robot.transform.rotation.eulerAngles.y;
            textMeshProUgui.text = $"{heading:0}° {DegreesToCardinal(heading)}";
        }
    }
}
