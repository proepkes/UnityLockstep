using Lockstep.Network.Utils;

namespace Lockstep.Network.Messages
{
    public class Init : ISerializable
    {
        public int Seed { get; set; }

        public byte ActorID { get; set; }

        public byte[] AllActors { get; set; }

        public int TargetFPS { get; set; } 

        public void Serialize(Serializer writer)
        {
            writer.Put(Seed);
            writer.Put(ActorID);
            writer.PutBytesWithLength(AllActors);
            writer.Put(TargetFPS);
        }

        public void Deserialize(Deserializer reader)
        {
            Seed = reader.GetInt();
            ActorID = reader.GetByte();
            AllActors = reader.GetBytesWithLength();
            TargetFPS = reader.GetInt();
        }
    }
}
