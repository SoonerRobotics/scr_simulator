using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class NRCConfigLoader : MonoBehaviour
    {

        private string robotName = "NRC";

        private SimpleCarController simpleCarController;
        private RosConnector rosConnector;
        private ImagePublisher imagePublisher;
        private IMUPublisher iMUPublisher;
        private VelocityPublisher velocityPublisher;
        private MotorsSubscriber motorsSubscriber;

        void Awake()
        {
            simpleCarController = this.GetComponent<SimpleCarController>();
            rosConnector = this.GetComponent<RosConnector>();
            imagePublisher = this.GetComponent<ImagePublisher>();
            iMUPublisher = this.GetComponent<IMUPublisher>();
            velocityPublisher = this.GetComponent<VelocityPublisher>();
            motorsSubscriber = this.GetComponent<MotorsSubscriber>();

            simpleCarController.useController = !RobotOptions.GetValue(robotName + "Autonomous").Equals("True");
            rosConnector.RosBridgeServerUrl = "ws://" + RobotOptions.GetValue(robotName + "ROS Bridge IP");
            imagePublisher.Topic = RobotOptions.GetValue(robotName + "Camera Topic");
            iMUPublisher.Topic = RobotOptions.GetValue(robotName + "IMU Topic");
            velocityPublisher.Topic = RobotOptions.GetValue(robotName + "Velocity Topic");
            motorsSubscriber.Topic = RobotOptions.GetValue(robotName + "Motors Topic");
        }
    }
}