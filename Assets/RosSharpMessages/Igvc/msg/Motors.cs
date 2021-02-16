/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */



namespace RosSharp.RosBridgeClient.MessageTypes.Igvc
{
    public class Motors : Message
    {
        public const string RosMessageName = "igvc_msgs/motors";

        public float left { get; set; }
        public float right { get; set; }

        public Motors()
        {
            this.left = 0.0f;
            this.right = 0.0f;
        }

        public Motors(float left, float right)
        {
            this.left = left;
            this.right = right;
        }
    }
}
