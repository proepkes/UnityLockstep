using BEPUutilities;
using ECS.Data;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : ICommand, ISerializable
    {                                                          
        public int EntityConfigId;

        public Vector2 Position;        

        public void Execute(InputContext context)
        {        
            var e = context.CreateEntity();    
            e.AddSpawnInputData(0, EntityConfigId, Position);   
        }

        public void Serialize(ISerializer writer)
        {
            writer.Put(EntityConfigId);   
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        public void Deserialize(IDeserializer reader)
        {
            EntityConfigId = reader.GetInt();         
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
