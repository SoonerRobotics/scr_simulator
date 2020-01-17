using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Level", menuName = "scr_simulator/Level", order = 1)]
public class LevelScriptableObject : ScriptableObject
{
    public string levelName;
    public int levelId;
}
