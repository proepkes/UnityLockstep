using BEPUutilities;       
using Lockstep.Framework.Networking;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : Command
    {                                                          
        public int EntityConfigId;

        public Vector2 Position;    

        public SpawnCommand() : base(CommandTag.Spawn)
        {
        }        

        public override void Execute(InputContext context)
        {        
            var e = context.CreateEntity();    
            e.AddSpawnInputData(0, EntityConfigId, Position);   
        }

        protected override void OnSerialize(ISerializer writer)
        {
            writer.Put(EntityConfigId);   
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        protected override void OnDeserialize(IDeserializer reader)
        {
            EntityConfigId = reader.GetInt();         
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
