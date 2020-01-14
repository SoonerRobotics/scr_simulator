using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public enum OptionTypes
{
    String, Bool
}

[Serializable]
public class Option
{
    public string name;
    public OptionTypes type;
}

[CreateAssetMenu(fileName = "Level", menuName = "scr_simulator/Level", order = 1)]
public class LevelScriptableObject : ScriptableObject
{
    public string levelName;
    public int levelId;
    public Option[] options;
}
