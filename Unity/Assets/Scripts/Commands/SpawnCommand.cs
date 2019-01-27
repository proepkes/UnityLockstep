using BEPUutilities;
using Lockstep.Client.Interfaces;
using Lockstep.Network.Utils;

namespace Lockstep.Commands
{
    public class SpawnCommand  : ISerializableCommand
    {
        public ushort Tag => 2;

        public int EntityConfigId;

        public Vector2 Position;        

        public void Execute(InputContext context)
        {        
            var e = context.CreateEntity();
            e.AddCoordinate(Position);
            e.AddEntityConfigId(EntityConfigId);
            e.AddPlayerId(0);
        }  

        public void Serialize(Serializer writer)
        {
            writer.Put(EntityConfigId);
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        public void Deserialize(Deserializer reader)
        {
            EntityConfigId = reader.GetInt();
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }

    }
}
