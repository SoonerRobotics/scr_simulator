using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject CameraTopic;
    public GameObject HeadingTopic;
    public GameObject VelocityTopic;
    public GameObject AccelerationTopic;
    public GameObject GpsTopic;
    public GameObject LaserScanTopic;
    public GameObject MotorsTopic;
    public GameObject RosBridgeTopic;
    public GameObject Autonomous;

    public void UpdateFields()
    {
        MenuValues.camera_topic = CameraTopic.GetComponent<TMP_InputField>().text;
        MenuValues.heading_topic = HeadingTopic.GetComponent<TMP_InputField>().text;
        MenuValues.velocity_topic = VelocityTopic.GetComponent<TMP_InputField>().text;
        MenuValues.acceleration_topic = AccelerationTopic.GetComponent<TMP_InputField>().text;
        MenuValues.gps_topic = GpsTopic.GetComponent<TMP_InputField>().text;
        MenuValues.laser_scan_topic = LaserScanTopic.GetComponent<TMP_InputField>().text;
        MenuValues.motors_topic = MotorsTopic.GetComponent<TMP_InputField>().text;
        MenuValues.ros_bridge_url = RosBridgeTopic.GetComponent<TMP_InputField>().text;
        MenuValues.autonomous = Autonomous.GetComponent<Toggle>().isOn;

        Debug.Log(MenuValues.camera_topic + MenuValues.heading_topic + MenuValues.velocity_topic + MenuValues.acceleration_topic +
             MenuValues.gps_topic + MenuValues.laser_scan_topic + MenuValues.motors_topic + MenuValues.ros_bridge_url + MenuValues.autonomous);
    }

    public void PlaySim()
    {
        SceneManager.LoadScene(1);
    }
}
