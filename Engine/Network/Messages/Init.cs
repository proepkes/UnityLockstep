using LiteNetLib.Utils;
using Lockstep.Network.Utils;

namespace Lockstep.Network.Messages
{
    public class Init : ISerializable
    {
        public int Seed { get; set; }

        public byte PlayerID { get; set; }

        public int TargetFPS { get; set; } 

        public void Serialize(Serializer writer)
        {
            writer.Put(Seed);
            writer.Put(PlayerID);
            writer.Put(TargetFPS);
        }

        public void Deserialize(Deserializer reader)
        {
            Seed = reader.GetInt();
            PlayerID = reader.GetByte();
            TargetFPS = reader.GetInt();
        }
    }
}
