using BEPUutilities;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Commands
{
    public class SpawnCommand  : ISerilalizableCommand
    {
        public string AssetName;

        public Vector2 Position;

        public void Serialize(NetDataWriter writer)
        {                               
            writer.Put(AssetName);
            writer.Put((long)Position.X);
            writer.Put((long)Position.Y);
        }

        public void Deserialize(NetDataReader reader)
        {
            AssetName = reader.GetString();
            Position.X = reader.GetLong();
            Position.Y = reader.GetLong();
        }

        public void Execute(InputContext context)
        {
            var e = context.CreateEntity();
            e.AddSpawnInput(AssetName);
        }
    }
}
