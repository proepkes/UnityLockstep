using BEPUutilities;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Commands
{
    public class NavigateCommand : ISerilalizableCommand
    {
        public int[] EntityIds;

        public Vector2 Destination;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(EntityIds);
            writer.Put((long)Destination.X);
            writer.Put((long)Destination.Y);
        }

        public void Deserialize(NetDataReader reader)
        {
            EntityIds = reader.GetIntArray();
            Destination.X = reader.GetLong();
            Destination.Y = reader.GetLong();
        }
    }
}
