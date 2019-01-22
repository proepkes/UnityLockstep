using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Networking.Messages
{
    public class HashCode
    {
        public ulong FrameNumber { get; set; }
        public long Value { get; set; }

        public void Serialize(ISerializer writer)
        {
            writer.Put(FrameNumber);
            writer.Put(Value);
        }

        public void Deserialize(IDeserializer reader)
        {

            FrameNumber = reader.GetULong();
            Value = reader.GetLong();
        }
    }
}