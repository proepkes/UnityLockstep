using BEPUutilities;
using LiteNetLib.Utils;
using Lockstep.Framework.Services;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : CommandBase
    {
        public string AssetName;

        public bool RegisterToPathfinder;

        public Vector2 Position;


        public SpawnCommand() : base(CommandTag.Spawn)
        {
        }   

        public override void Execute(GameContext context)
        {        
            var e = context.CreateEntity();
            e.AddAsset(AssetName);
            e.AddPosition(Position);
            if (RegisterToPathfinder)
            {
                e.isNavigationAware = true;
            }
        }

        protected override void OnSerialize(NetDataWriter writer)
        {
            writer.Put(AssetName);
            writer.Put(RegisterToPathfinder);
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        protected override void OnDeserialize(NetDataReader reader)
        {
            AssetName = reader.GetString();
            RegisterToPathfinder = reader.GetBool();
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }     
    }
}
