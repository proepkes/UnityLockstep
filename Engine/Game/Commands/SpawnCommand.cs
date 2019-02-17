using System;
using BEPUutilities;
using Lockstep.Core.Logic.Interfaces;
using Lockstep.Core.Logic.Serialization.Utils;

namespace Lockstep.Game.Commands
{
    [Serializable]
    public class SpawnCommand : ICommand
    {
        public ushort Tag => 2;

        public int DatabaseEntityId;

        public Vector2 Position;

        public void Execute(InputEntity e)
        {
            e.AddCoordinate(Position);
            e.AddDatabaseEntityId(DatabaseEntityId);
        }

        public void Serialize(Serializer writer)
        {
            writer.Put(DatabaseEntityId);
            writer.Put(Position.X.RawValue);
            writer.Put(Position.Y.RawValue);
        }

        public void Deserialize(Deserializer reader)
        {
            DatabaseEntityId = reader.GetInt();
            Position.X.RawValue = reader.GetLong();
            Position.Y.RawValue = reader.GetLong();
        }
    }
}
