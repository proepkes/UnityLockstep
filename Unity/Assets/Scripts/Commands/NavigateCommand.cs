using BEPUutilities;
using ECS.Data;
using Lockstep.Framework.Networking.Serialization;

namespace Lockstep.Framework.Commands
{
    public class NavigateCommand : ICommand, ISerializable
    {
        public int[] EntityIds;

        public Vector2 Destination;

        public void Serialize(ISerializer writer)
        {
            writer.PutArray(EntityIds);
            writer.Put((long)Destination.X);
            writer.Put((long)Destination.Y);
        }

        public void Deserialize(IDeserializer reader)
        {
            EntityIds = reader.GetIntArray();
            Destination.X = reader.GetLong();
            Destination.Y = reader.GetLong();
        }

        public void Execute(InputContext context)
        {
            var e = context.CreateEntity();
            e.AddNavigationInputData(EntityIds, Destination);                              
        }
    }
}
