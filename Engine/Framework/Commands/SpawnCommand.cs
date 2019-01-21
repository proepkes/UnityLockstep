using BEPUutilities;    
using LiteNetLib.Utils;
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

        public override void Execute(GameContext context)
        {        
            var e = context.CreateEntity();    
            e.AddPosition(Position);    
            e.AddConfigId(EntityConfigId);
        }

        protected override void OnSerialize(NetDataWriter writer)
        {
            writer.Put(EntityConfigId);   
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        protected override void OnDeserialize(NetDataReader reader)
        {
            EntityConfigId = reader.GetInt();         
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
