namespace Messages
{
    public enum MessageType : ushort
    {
        // 2026
        ImageFrame = 0x01,
        ArcLog = 0x02,
        Metric = 0x04,
        MetricHistory = 0x05,
        CanFrame = 0x07,
        VectorNav = 0x08,
        ZedDepth = 0x09,
        
        // Reserved Stuff
        CapabilityReq = 60_000,
        CapabilityAck = 60_001,
        CommandReq = 60_002,
        CommandAck = 60_003,
    }
}