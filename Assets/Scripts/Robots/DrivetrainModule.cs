using RosMessageTypes.IgvcMessages;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class DrivetrainBehaviour : MonoBehaviour
{
    private ROSConnection _rosConnection;

    public string InputTopic = "/igvc/motor_input";    
    public string FeedbackTopic = "/igvc/motor_feedback";
    
    private MotorInputMsg _lastMotorInput;
    private Vector3 _lastPosition;
    private Vector3 _lastRotation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<MotorInputMsg>(InputTopic, OnMotorInputReceived);
    }

    void OnMotorInputReceived(MotorInputMsg msg)
    {
        _lastMotorInput = msg;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 localVelocity = new Vector2(_lastMotorInput.sideways_velocity, _lastMotorInput.forward_velocity);
        float localAngular = _lastMotorInput.angular_velocity;
        
        // store last position/rotation
        Vector3 currentPosition = _lastPosition;
        Vector3 currentRotation = _lastRotation;

        // perform translation/rotation
        transform.Translate(localVelocity.x * Time.deltaTime, 0, localVelocity.y * Time.deltaTime);
        transform.Rotate(0, localAngular * Time.deltaTime, 0);
        
        // send feedback
        MotorFeedbackMsg msg = new()
        {
            delta_x = currentPosition.x - _lastPosition.x,
            delta_y = currentPosition.y - _lastPosition.y,
            delta_theta = currentRotation.y - _lastRotation.y,
        };
        _rosConnection.Publish(FeedbackTopic, msg);
        
        // update last known position/rotation
        _lastPosition = currentPosition;
        _lastRotation = currentRotation;
    }
}
