using BEPUutilities;
using LiteNetLib.Utils;
using Lockstep.Framework.Services;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : CommandBase
    {
        public string AssetName;

        public bool Movable;

        public Vector2 Position;


        public SpawnCommand() : base(CommandTag.Spawn)
        {
        }   

        public override void Execute(InputContext context)
        {        
            var e = context.CreateEntity();
            e.AddSpawnInput(AssetName, Movable);
            e.AddInputPosition(Position);
        }

        protected override void OnSerialize(NetDataWriter writer)
        {
            writer.Put(AssetName);
            writer.Put(Movable);
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        protected override void OnDeserialize(NetDataReader reader)
        {
            AssetName = reader.GetString();
            Movable = reader.GetBool();
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
