using BEPUutilities;     
using Lockstep.Client.Interfaces;
using Lockstep.Network.Utils;

namespace Lockstep.Commands
{
    public class NavigateCommand : ISerializableCommand
    {
        public ushort Tag => 1;

        public uint[] EntityIds;

        public Vector2 Destination;    

        public void Execute(InputEntity e)
        {                                  
            e.AddEntityIds(EntityIds);
            e.AddCoordinate(Destination);                           
        }

        public void Serialize(Serializer writer)
        {
            writer.PutArray(EntityIds);
            writer.Put(Destination.X.RawValue);
            writer.Put(Destination.Y.RawValue);
        }

        public void Deserialize(Deserializer reader)
        {
            EntityIds = reader.GetUIntArray();
            Destination.X.RawValue = reader.GetLong();
            Destination.Y.RawValue = reader.GetLong();
        }

    }
}
