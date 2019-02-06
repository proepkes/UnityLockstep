using Lockstep.Core.Commands;
using Lockstep.Network;

namespace Lockstep.Game.Network
{
    public interface ISerializableCommand : ICommand, ISerializable
    {
        ushort Tag { get; }

    }
}