/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



namespace RosSharp.RosBridgeClient.MessageTypes.Autonav
{
    public class GPSFeedback : Message
    {
        public const string RosMessageName = "autonav_msgs/GPSFeedback";

        public double latitude { get; set; }
        public double longitude { get; set; }
        public bool is_locked { get; set; }

        public GPSFeedback()
        {
            this.latitude = 0.0;
            this.longitude = 0.0;
            this.is_locked = false;
        }

        public GPSFeedback(double latitude, double longitude, bool is_locked)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.is_locked = is_locked;
        }
    }
}
