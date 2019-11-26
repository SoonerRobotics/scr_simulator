using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MenuValues
{

    [NonSerialized]
    public static MenuValues _instance; 

    public string autonomous;
    public string camera_topic;
    public string imu_topic;
    public string velocity_topic;
    public string gps_topic;
    public string laser_scan_topic;
    public string motors_topic;
    public string ros_bridge_url;
}
