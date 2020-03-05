using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFG_Motors
{
    [JsonProperty("Use Angular Velocity")]
    public bool useAngularVelocity = true;

    [JsonProperty("Velocity Decay")]
    public float velocityDecay = 0.15f;
}

public class CFG_ManualControl
{
    [JsonProperty("Full Speed")]
    public float fullSpeed = 2.27f;
}

public class ControlConfig : ConfigFile
{

    [JsonProperty("Motors")]
    public CFG_Motors motors = new CFG_Motors();

    [JsonProperty("Manual Control")]
    public CFG_ManualControl manual = new CFG_ManualControl();

    /// <summary>
    /// Creates/Loads a new configuration file
    /// </summary>
    /// <param name="file">The name of the file itself, do not include the extension.</param>
    public ControlConfig(string file) : base(file)
    {
        Populate(this);
        Save(); // Ensure the file is saved at somepoint. Otherwise it wont ever exist :(
    }

    /// <summary>
    /// Save the configuration file
    /// </summary>
    internal void Save()
    {
        Save(this);
    }
}
