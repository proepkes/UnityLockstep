using Lockstep.Core.Logic.Serialization;
using Lockstep.Core.Logic.Serialization.Utils;

namespace Lockstep.Network.Messages
{
    public class HashCode : ISerializable
    {
        public ulong Tick { get; set; }
        public long Value { get; set; }       

        public void Serialize(Serializer writer)
        {
            writer.Put(Tick);
            writer.Put(Value);
        }

        public void Deserialize(Deserializer reader)
        {
            Tick = reader.GetULong();
            Value = reader.GetLong();
        }
    }
}