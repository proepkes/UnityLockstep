using Lockstep.Network;

namespace Lockstep.Game.Commands
{
    public interface ISerializableCommand : ICommand, ISerializable
    {
        ushort Tag { get; }

    }
}