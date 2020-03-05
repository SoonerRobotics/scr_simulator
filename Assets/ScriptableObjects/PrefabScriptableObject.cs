using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Prefab", menuName = "scr_simulator/Map Editor", order = 2)]
public class PrefabScriptableObject : ScriptableObject
{
    public string Name;
    public string Identifier;
    public GameObject Reference;
}
