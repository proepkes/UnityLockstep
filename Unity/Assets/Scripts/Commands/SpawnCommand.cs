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
            writer.Put(Position.X.RawValue);
            writer.Put(Position.Y.RawValue);
        }

        public void Deserialize(Deserializer reader)
        {
            EntityConfigId = reader.GetInt();
            Position.X.RawValue = reader.GetLong();
            Position.Y.RawValue = reader.GetLong();
        }

    }
}
