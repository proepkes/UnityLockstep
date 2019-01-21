namespace Lockstep.Framework.Networking.Serialization
{
    public class HashCode
    {
        public ulong FrameNumber { get; set; }
        public long Value { get; set; }

        public void Serialize(INetworkWriter writer)
        {
            writer.Put(FrameNumber);
            writer.Put(Value);
        }

        public void Deserialize(INetworkReader reader)
        {

            FrameNumber = reader.GetULong();
            Value = reader.GetLong();
        }
    }
}