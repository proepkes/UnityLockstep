using BEPUutilities;       
using Lockstep.Framework.Networking;
using Lockstep.Framework.Services;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : CommandBase
    {                                                          
        public int EntityConfigId;

        public Vector2 Position;    

        public SpawnCommand() : base(CommandTag.Spawn)
        {
        }        

        public override void Execute(InputContext context)
        {        
            var e = context.CreateEntity();    
            e.AddSpawnInputData(IssuerId, EntityConfigId, Position);   
        }

        protected override void OnSerialize(INetworkWriter writer)
        {
            writer.Put(EntityConfigId);   
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        protected override void OnDeserialize(INetworkReader reader)
        {
            EntityConfigId = reader.GetInt();         
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
