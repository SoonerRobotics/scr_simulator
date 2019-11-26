using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class IGVCConfigLoader : MonoBehaviour
    {

        private SimpleCarController simpleCarController;
        private RosConnector rosConnector;
        private ImagePublisher imagePublisher;
        private LaserScanPublisher laserScanPublisher;
        private IMUPublisher iMUPublisher;
        private VelocityPublisher velocityPublisher;
        private GPSPublisher gPSPublisher;
        private MotorsSubscriber motorsSubscriber;

        void Start()
        {
            if (MenuValues._instance != null)
            {
                simpleCarController = this.GetComponent<SimpleCarController>();
                rosConnector = this.GetComponent<RosConnector>();
                imagePublisher = this.GetComponent<ImagePublisher>();
                laserScanPublisher = this.GetComponent<LaserScanPublisher>();
                iMUPublisher = this.GetComponent<IMUPublisher>();
                velocityPublisher = this.GetComponent<VelocityPublisher>();
                gPSPublisher = this.GetComponent<GPSPublisher>();
                motorsSubscriber = this.GetComponent<MotorsSubscriber>();

                simpleCarController.useController = !MenuValues._instance.autonomous.Equals("True");
                rosConnector.RosBridgeServerUrl = "ws://" + MenuValues._instance.ros_bridge_url;
                imagePublisher.Topic = MenuValues._instance.camera_topic;
                laserScanPublisher.Topic = MenuValues._instance.laser_scan_topic;
                iMUPublisher.Topic = MenuValues._instance.imu_topic;
                velocityPublisher.Topic = MenuValues._instance.velocity_topic;
                gPSPublisher.Topic = MenuValues._instance.gps_topic;
                motorsSubscriber.Topic = MenuValues._instance.motors_topic;
            }
        }
    }
}