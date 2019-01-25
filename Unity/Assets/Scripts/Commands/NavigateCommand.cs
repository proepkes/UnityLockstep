using BEPUutilities;     
using Lockstep.Client.Interfaces;
using Lockstep.Network.Utils;

namespace Lockstep.Commands
{
    public class NavigateCommand : ISerializableCommand
    {
        public ushort Tag => 1;

        public int[] EntityIds;

        public Vector2 Destination;    

        public void Execute(InputContext context)
        {
            var e = context.CreateEntity();
            e.AddNavigationInputData(EntityIds, Destination);                              
        }

        public void Serialize(Serializer writer)
        {
            writer.PutArray(EntityIds);
            writer.Put((long)Destination.X);
            writer.Put((long)Destination.Y);
        }

        public void Deserialize(Deserializer reader)
        {
            EntityIds = reader.GetIntArray();
            Destination.X = reader.GetLong();
            Destination.Y = reader.GetLong();
        }

    }
}
