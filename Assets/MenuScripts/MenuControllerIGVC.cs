using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

public class MenuControllerIGVC : MonoBehaviour
{

    public string jsonPath = "config.json";

    public GameObject CameraTopic;
    public GameObject IMUTopic;
    public GameObject VelocityTopic;
    public GameObject GpsTopic;
    public GameObject LaserScanTopic;
    public GameObject MotorsTopic;
    public GameObject RosBridgeTopic;
    public GameObject Autonomous;
    //public GameObject LevelID;

    public void Start()
    {

        if (File.Exists(jsonPath))
        {
            StreamReader reader = new StreamReader(jsonPath);
            string json_raw = reader.ReadToEnd();
            MenuValues._instance = JsonUtility.FromJson<MenuValues>(json_raw);
            reader.Close();
        } 
        else
        {
            MenuValues._instance = new MenuValues
            {
                autonomous = "True",
                camera_topic = "/igvc/camera/compressed",
                imu_topic = "/igvc/imu",
                velocity_topic = "/igvc/velocity",
                gps_topic = "/igvc/gps",
                laser_scan_topic = "/igvc/lidar",
                motors_topic = "/igvc/motors_raw",
                ros_bridge_url = "localhost:9090",
                //level_id = "1"

            };
            StreamWriter writer = new StreamWriter(jsonPath, true);
            writer.WriteLine(JsonUtility.ToJson(MenuValues._instance, true));
            writer.Close();
        }

        CameraTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.camera_topic;
        IMUTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.imu_topic;
        VelocityTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.velocity_topic;
        GpsTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.gps_topic;
        LaserScanTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.laser_scan_topic;
        MotorsTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.motors_topic;
        RosBridgeTopic.GetComponent<TMP_InputField>().text = MenuValues._instance.ros_bridge_url;
        //LevelID.GetComponent<TMP_InputField>().text = MenuValues._instance.level_id;
        Autonomous.GetComponent<Toggle>().isOn = MenuValues._instance.autonomous.Equals("True");
    }

    public void UpdateDemFieldsYo()
    {
        MenuValues._instance.camera_topic = CameraTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.imu_topic = IMUTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.velocity_topic = VelocityTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.gps_topic = GpsTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.laser_scan_topic = LaserScanTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.motors_topic = MotorsTopic.GetComponent<TMP_InputField>().text;
        MenuValues._instance.ros_bridge_url = RosBridgeTopic.GetComponent<TMP_InputField>().text;
        //MenuValues._instance.level_id = LevelID.GetComponent<TMP_InputField>().text;
        MenuValues._instance.autonomous = Autonomous.GetComponent<Toggle>().isOn.ToString();
    }

    public void PlaySim()
    {
        UpdateDemFieldsYo();
        SceneManager.LoadScene(Int32.Parse(MenuValues._instance.level_id));
    }
}
