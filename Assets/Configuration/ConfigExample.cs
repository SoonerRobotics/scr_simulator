using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigExample : ConfigFile
{
    [JsonProperty("Camera Speed")] // How it is show in the config file
    public float cameraSpeed = 0.1f; // 0.1f is our default value

    [JsonProperty("Caridnals Directions")]
    public string[] caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

    /// <summary>
    /// Creates/Loads a new configuration file
    /// </summary>
    /// <param name="file">The name of the file itself, do not include the extension.</param>
    public ConfigExample(string file) : base(file)
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
