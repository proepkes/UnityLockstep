using LiteNetLib.Utils;

namespace Lockstep.Framework.Commands
{
    public interface ISerilalizableCommand
    {
        void Serialize(NetDataWriter writer);

        void Deserialize(NetDataReader reader);

        void Execute(GameContext context);
    }
}