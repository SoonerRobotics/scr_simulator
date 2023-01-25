using RosSharp.RosBridgeClient.MessageTypes.Autonav;
using RosSharp.RosBridgeClient.MessageTypes.Igvc;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class IGVCConfigLoader : MonoBehaviour
    {

        public string robotName = "IGVC";
        public Camera mainCamera;
        public Camera robotCamera;

        private SimpleCarController simpleCarController;
        private ImagePublisher imagePublisher;
        private IMUPublisher iMUPublisher;
        private VelocityPublisherIGVC23 velocityPublisher;
        private GPSPublisher gPSPublisher;
        private IGVCMotorsSubscriberIGVC23 motorsSubscriber;


        void Awake()
        {
            simpleCarController = this.GetComponent<SimpleCarController>();
            imagePublisher = this.GetComponent<ImagePublisher>();
            iMUPublisher = this.GetComponent<IMUPublisher>();
            velocityPublisher = this.GetComponent<VelocityPublisherIGVC23>();
            gPSPublisher = this.GetComponent<GPSPublisher>();
            motorsSubscriber = this.GetComponent<IGVCMotorsSubscriberIGVC23>();

            simpleCarController.useController = !RobotOptions.GetValue(robotName + "Autonomous").Equals("True");
            //rosConnector.RosBridgeServerUrl = "ws://" + RobotOptions.GetValue(robotName + "ROS Bridge IP");
            if (RobotOptions.Exists(robotName + "Camera Topic"))
            {
                imagePublisher.Topic = RobotOptions.GetValue(robotName + "Camera Topic");
            }
            if (RobotOptions.Exists(robotName + "Show Camera View") && RobotOptions.GetValue(robotName + "Show Camera View").Equals("True")) {
                robotCamera.targetDisplay = 0;
                robotCamera.enabled = true;
                mainCamera.enabled = false;

                // If we are publishing the camera, we need to duplicate it because the publishing script
                // will override the camera output.
                if(RobotOptions.GetValue(robotName + "Publish Camera").Equals("True")) {
                    GameObject robotCameraDupe = Instantiate(robotCamera.gameObject, robotCamera.gameObject.transform.parent);
                }
            }
            if (RobotOptions.GetValue(robotName + "Publish Camera").Equals("True")) {
                imagePublisher.enabled = true;
                robotCamera.enabled = true;
            }
            if (RobotOptions.Exists(robotName + "IMU Topic")) {
                iMUPublisher.Topic = RobotOptions.GetValue(robotName + "IMU Topic");
            }
            velocityPublisher.Topic = RobotOptions.GetValue(robotName + "Velocity Topic");
            gPSPublisher.Topic = RobotOptions.GetValue(robotName + "GPS Topic");
            motorsSubscriber.Topic = RobotOptions.GetValue(robotName + "Motors Topic");
        }
    }
}