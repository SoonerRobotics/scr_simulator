using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Option
{
    public string name;
    public string defaultValue;
}

[CreateAssetMenu(fileName = "Robot", menuName = "scr_simulator/Robot", order = 1)]
public class RobotScriptableObject : ScriptableObject
{
    public string robotName;
    public GameObject robotPrefab;
    public Option[] options;
}
