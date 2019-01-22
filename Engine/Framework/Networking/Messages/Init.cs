using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Networking.Messages
{
    public class Init
    {
        public int Seed { get; set; }

        public byte PlayerID { get; set; }

        public int TargetFPS { get; set; }

        public void Serialize(ISerializer writer)
        {
            writer.Put(Seed);
            writer.Put(PlayerID);
            writer.Put(TargetFPS);
        }

        public void Deserialize(IDeserializer reader)
        {
            Seed = reader.GetInt();
            PlayerID = reader.GetByte();
            TargetFPS = reader.GetInt();
        }
    }
}
