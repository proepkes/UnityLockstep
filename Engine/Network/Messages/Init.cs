using Lockstep.Core.Logic.Serialization;
using Lockstep.Core.Logic.Serialization.Utils;

namespace Lockstep.Network.Messages
{
    public class Init : ISerializable
    {
        public int Seed { get; set; }

        public byte ActorID { get; set; }

        public byte[] AllActors { get; set; }

        public int SimulationSpeed { get; set; } 
                                              
        public void Serialize(Serializer writer)
        {
            writer.Put(Seed);
            writer.Put(ActorID);
            writer.PutBytesWithLength(AllActors);
            writer.Put(SimulationSpeed);
        }

        public void Deserialize(Deserializer reader)
        {
            Seed = reader.GetInt();
            ActorID = reader.GetByte();
            AllActors = reader.GetBytesWithLength();
            SimulationSpeed = reader.GetInt();
        }
    }
}
