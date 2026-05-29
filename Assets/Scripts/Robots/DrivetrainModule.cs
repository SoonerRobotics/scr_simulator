using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;

namespace Robots
{
    public class DrivetrainBehaviour : MonoBehaviour
    {
        private CanFrame? _mLastCanFrame;
        private Vector3 _mLastPosition;
        private Vector3 _mLastRotation;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            SusConnection.Instance.Subscribe(MessageType.CanFrame, OnCanFrameReceived);
        }

        private void OnCanFrameReceived(MessageWrapper wrapper)
        {
            var frame = CanFrame.GetRootAsCanFrame(
                new ByteBuffer(wrapper.Data)
            );
            _mLastCanFrame = frame;
        }

        // Update is called once per frame
        private void Update()
        {
            if (_mLastCanFrame?.CanId != 0xA)
            {
                return;
            }
            
            // Extract the actual motor command
            var motorCommand = CanHelper.PacketFromBytes<CanHelper.MotorControlPacket>(_mLastCanFrame.Value.GetCanDataArray());
            
            var localVelocity = new Vector2(motorCommand.SidewaysVelocity * 0.001f, motorCommand.ForwardVelocity * 0.001f);
            var localAngular = motorCommand.AngularVelocity * 0.001f;
            // convert localAngular from radians per second to degrees per second
            localAngular *= -Mathf.Rad2Deg;
            Debug.Log($"Received motor command: {localVelocity} m/s, {localAngular} deg/s");
        
            // store last position/rotationss
            var currentPosition = _mLastPosition;
            var currentRotation = _mLastRotation;
            
            // perform translation/rotation
            transform.Translate(localVelocity.x * Time.deltaTime, 0, localVelocity.y * Time.deltaTime);
            transform.Rotate(0, localAngular * Time.deltaTime, 0);
        
            // send feedback
            var msg = new CanHelper.MotorOdometryPacket()
            {
                RawDeltaX = (short)(currentPosition.x - _mLastPosition.x),
                RawDeltaY = (short)(currentPosition.y - _mLastPosition.y),
                RawDeltaTheta = (short)(currentRotation.y - _mLastRotation.y)
            };
            var byts = CanHelper.FromMotorOdometry(msg);
            var builder = new FlatBufferBuilder(1024);
            var canDataOffset = CanFrame.CreateCanDataVector(builder, byts);
            var imageOffset = CanFrame.CreateCanFrame(
                builder,
                (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                0,
                0xB,
                canDataOffset
            );
            builder.Finish(imageOffset.Value);
            var wrapper = MessageWrapper.From(MessageType.CanFrame, builder.SizedByteArray());
            SusConnection.Instance.Broadcast(wrapper);
            
            // update last known position/rotation
            _mLastPosition = currentPosition;
            _mLastRotation = currentRotation;
            _mLastCanFrame = null;
        }
    }
}
