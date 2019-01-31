using System;
using BEPUutilities;     
using Lockstep.Client.Interfaces;
using Lockstep.Core.Data;
using Lockstep.Network.Utils;

namespace Lockstep.Commands
{
    public class NavigateCommand : ISerializableCommand
    {
        public ushort Tag => 1;

        public EntityId[] Selection;

        public Vector2 Destination;    

        public void Execute(InputEntity e)
        {

            e.AddSelection(Selection);
            e.AddCoordinate(Destination);                           
        }

        public void Serialize(Serializer writer)
        {
            var selection = new uint[Selection.Length];
            Array.Copy(Selection, selection, Selection.Length);    

            writer.PutArray(selection);
            writer.Put(Destination.X.RawValue);
            writer.Put(Destination.Y.RawValue);
        }

        public void Deserialize(Deserializer reader)
        {
            var selection = reader.GetUIntArray(); 
            Selection = new EntityId[selection.Length];
            Array.Copy(selection, Selection, selection.Length);

            Destination.X.RawValue = reader.GetLong();
            Destination.Y.RawValue = reader.GetLong();
        }

    }
}
