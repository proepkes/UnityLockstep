using BEPUutilities;
using LiteNetLib.Utils;

namespace Lockstep.Framework.Commands
{
    public class NavigateCommand
    {
        public int[] entityIds;

        public Vector2 destination;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(entityIds);
            writer.Put((long) destination.X);
            writer.Put((long) destination.Y);
        }

        public void Deserialize(NetDataReader reader)
        {
            entityIds = reader.GetIntArray();
            destination.X = reader.GetLong();
            destination.Y = reader.GetLong();
        }
    }
}
