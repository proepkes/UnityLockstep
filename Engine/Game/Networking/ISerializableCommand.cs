using Lockstep.Core.Commands;
using Lockstep.Network;

namespace Lockstep.Game.Networking
{
    public interface ISerializableCommand : ICommand, ISerializable
    {
        ushort Tag { get; }

    }
}