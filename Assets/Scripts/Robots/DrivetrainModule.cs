using SUS;
using UnityEngine;

namespace Robots
{
    public class DrivetrainBehaviour : MonoBehaviour
    {
        // private IncomingMotorInput _mLastMotorInput;
        private Vector3 _mLastPosition;
        private Vector3 _mLastRotation;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            // SusConnection.Instance.Subscribe<IncomingMotorInput>(OnMotorInputReceived);
        }

        // private void OnMotorInputReceived(IncomingMotorInput msg)
        // {
        //     _mLastMotorInput = msg;
        // }

        // Update is called once per frame
        private void Update()
        {
            // if (_mLastMotorInput == null) return;
            //
            // var localVelocity = new Vector2(_mLastMotorInput.SidewaysVelocity, _mLastMotorInput.ForwardVelocity);
            // var localAngular = _mLastMotorInput.AngularVelocity;
        
            // store last position/rotation
            // var currentPosition = _mLastPosition;
            // var currentRotation = _mLastRotation;
            //
            // // perform translation/rotation
            // transform.Translate(localVelocity.x * Time.deltaTime, 0, localVelocity.y * Time.deltaTime);
            // transform.Rotate(0, localAngular * Time.deltaTime, 0);
        
            // send feedback
            // var msg = new OutgoingMotorFeedback(
            //     currentPosition.x - _mLastPosition.x,
            //     currentPosition.y - _mLastPosition.y,
            //     currentRotation.y - _mLastRotation.y
            // );
            // SusConnection.Instance.Write(msg);
        
            // update last known position/rotation
            // _mLastPosition = currentPosition;
            // _mLastRotation = currentRotation;
        }
    }
}
