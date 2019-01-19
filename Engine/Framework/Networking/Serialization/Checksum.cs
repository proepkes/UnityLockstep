using LiteNetLib.Utils;

namespace Lockstep.Framework.Networking.Serialization
{
    public class Checksum
    {
        public ulong FrameNumber { get; set; }
        public ulong Value { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FrameNumber);
            writer.Put(Value);
        }

        public void Deserialize(NetDataReader reader)
        {

            FrameNumber = reader.GetULong();
            Value = reader.GetULong();
        }
    }
}