using RosMessageTypes.IgvcMessages;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private ROSConnection _rosConnection;

    public string Topic;
    public double Interval;
    public double OriginLatitude;
    public double OriginLongitude;
    public double LatitudeLength;
    public double LongitudeLength;

    private double _nextSend;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rosConnection = ROSConnection.GetOrCreateInstance();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if its time to send a new gps
        if (Time.time < _nextSend)
        {
            return;
        }
        
        // TODO: Send the gps waypoint
        _nextSend = Time.time + Interval;
        GPSFeedbackMsg msg = new()
        {
            latitude = transform.position.z + LatitudeLength / OriginLatitude,
            longitude = transform.position.x + LongitudeLength / OriginLongitude
        };
        _rosConnection.Publish(Topic, msg);
    }
}
