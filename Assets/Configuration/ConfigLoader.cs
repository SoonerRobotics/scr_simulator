using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Singletons could maybe improve this from having to be script ingame?
/// </summary>
public class ConfigLoader : MonoBehaviour
{
    public static ConfigLoader Instance;

    public SensorsConfig sensors;
    public ControlConfig control;

    void Start()
    {
        if (Instance)
        {
            return;
        }
        Instance = this;

        sensors = new SensorsConfig("sensors");
        control = new ControlConfig("control");

        DontDestroyOnLoad(this);
    }
}
