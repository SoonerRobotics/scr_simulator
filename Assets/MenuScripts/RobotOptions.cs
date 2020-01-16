using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotOptions
{
    public static Dictionary<string, string> dict = new Dictionary<string, string>();

    public static void LoadSaved(RobotScriptableObject[] robots)
    {
        foreach (RobotScriptableObject robot in robots)
        {
            foreach (Option option in robot.options)
            {
                string key = robot.robotName + option.name;
                dict[key] = PlayerPrefs.GetString(key, option.defaultValue);
            }
        }
    }

    public static void LoadDefaults(RobotScriptableObject[] robots)
    {
        foreach (RobotScriptableObject robot in robots)
        {
            foreach (Option option in robot.options)
            {
                string key = robot.robotName + option.name;
                dict[key] = option.defaultValue;
            }
        }
    }

    public static string GetValue(string key)
    {
        return dict[key];
    }

    public static void SetValue(string key, string value)
    {
        dict[key] = value;
    }

    public static void Save()
    {
        foreach (string key in dict.Keys)
        {
            PlayerPrefs.SetString(key, dict[key]);
        }
    }
}
