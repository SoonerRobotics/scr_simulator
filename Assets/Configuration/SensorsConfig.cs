using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFG_LIDAR
{
    [JsonProperty("Distance Noise Std Dev")]
    public float distanceNoise = 0.1f;
}

public class CFG_IMU
{
    [JsonProperty("Acceleration Noise Std Dev")]
    public float accelNoise = 0.15f;

    [JsonProperty("Orientation Noise Std Dev")]
    public float orientationNoise = 0.017f;

    [JsonProperty("Angular Velocity Noise Std Dev")]
    public float angularVelocityNoise = 0.017f;
}

public class CFG_Encoders
{
    [JsonProperty("Velocity Noise Std Dev")]
    public float velocityNoise = 0.05f;
}

public class CFG_GPS
{
    [JsonProperty("Latitude Starting Pos")]
    public float latStart = 35.194881f;

    [JsonProperty("Longitude Starting Pos")]
    public float lonStart = -97.438621f;

    [JsonProperty("Latitude Noise Std Dev")]
    public float latNoise = 1.843f;

    [JsonProperty("Longitude Noise Std Dev")]
    public float lonNoise = 2.138f;
}

public class SensorsConfig : ConfigFile
{

    [JsonProperty("LIDAR")]
    public CFG_LIDAR lidar = new CFG_LIDAR();

    [JsonProperty("IMU")]
    public CFG_IMU imu = new CFG_IMU();

    [JsonProperty("Encoders")]
    public CFG_Encoders encoders = new CFG_Encoders();

    [JsonProperty("GPS")]
    public CFG_GPS gps = new CFG_GPS();

    /// <summary>
    /// Creates/Loads a new configuration file
    /// </summary>
    /// <param name="file">The name of the file itself, do not include the extension.</param>
    public SensorsConfig(string file) : base(file)
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
