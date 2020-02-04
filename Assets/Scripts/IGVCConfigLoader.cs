using RosSharp.RosBridgeClient.MessageTypes.Igvc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class IGVCConfigLoader : MonoBehaviour
    {

        private string robotName = "IGVC";

        private SimpleCarController simpleCarController;
        private RosConnector rosConnector;
        private ImagePublisher imagePublisher;
        private LaserScanPublisher laserScanPublisher;
        private IMUPublisher iMUPublisher;
        private VelocityPublisher velocityPublisher;
        private GPSPublisher gPSPublisher;
        private MotorsSubscriber motorsSubscriber;

        void Awake()
        {
            simpleCarController = this.GetComponent<SimpleCarController>();
            rosConnector = this.GetComponent<RosConnector>();
            imagePublisher = this.GetComponent<ImagePublisher>();
            laserScanPublisher = this.GetComponent<LaserScanPublisher>();
            iMUPublisher = this.GetComponent<IMUPublisher>();
            velocityPublisher = this.GetComponent<VelocityPublisher>();
            gPSPublisher = this.GetComponent<GPSPublisher>();
            motorsSubscriber = this.GetComponent<MotorsSubscriber>();

            simpleCarController.useController = !RobotOptions.GetValue(robotName + "Autonomous").Equals("True");
            rosConnector.RosBridgeServerUrl = "ws://" + RobotOptions.GetValue(robotName + "ROS Bridge IP");
            imagePublisher.Topic = RobotOptions.GetValue(robotName + "Camera Topic");
            laserScanPublisher.Topic = RobotOptions.GetValue(robotName + "Laser Scan Topic");
            iMUPublisher.Topic = RobotOptions.GetValue(robotName + "IMU Topic");
            velocityPublisher.Topic = RobotOptions.GetValue(robotName + "Velocity Topic");
            gPSPublisher.Topic = RobotOptions.GetValue(robotName + "GPS Topic");
            motorsSubscriber.Topic = RobotOptions.GetValue(robotName + "Motors Topic");
        }
    }
}