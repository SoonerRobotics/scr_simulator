using System;
using System.Runtime.InteropServices;

public class CanHelper
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MotorControlPacket
    {
        public short ForwardVelocity;
        public short SidewaysVelocity;
        public short AngularVelocity;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MotorOdometryPacket
    {
        public short RawDeltaX;
        public short RawDeltaY;
        public short RawDeltaTheta;
    }

    public static T PacketFromBytes<T>(byte[] bytes) where T : struct
    {
        return MemoryMarshal.Read<T>(bytes.AsSpan());
    }

    public static MotorControlPacket ReadMotorControl(byte[] bytes)
    {
        return PacketFromBytes<MotorControlPacket>(bytes);
    }

    public static byte[] FromMotorOdometry(MotorOdometryPacket packet)
    {
        return new byte[] {};
    }
}