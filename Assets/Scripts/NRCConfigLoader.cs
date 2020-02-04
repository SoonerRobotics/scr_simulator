using RosSharp.RosBridgeClient.MessageTypes.Nrc;
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
        private DriveStatusPublisher driveStatusPublisher;
        private DriveCommandSubscriber driveCommandSubscriber;

        void Awake()
        {
            simpleCarController = this.GetComponent<SimpleCarController>();
            rosConnector = this.GetComponent<RosConnector>();
            imagePublisher = this.GetComponent<ImagePublisher>();
            driveStatusPublisher = this.GetComponent<DriveStatusPublisher>();
            driveCommandSubscriber = this.GetComponent<DriveCommandSubscriber>();

            simpleCarController.useController = !RobotOptions.GetValue(robotName + "Autonomous").Equals("True");
            rosConnector.RosBridgeServerUrl = "ws://" + RobotOptions.GetValue(robotName + "ROS Bridge IP");
            imagePublisher.Topic = RobotOptions.GetValue(robotName + "Camera Topic");
            driveStatusPublisher.Topic = RobotOptions.GetValue(robotName + "Drive Status Topic");
            driveCommandSubscriber.Topic = RobotOptions.GetValue(robotName + "Drive Command Topic");
        }
    }
}