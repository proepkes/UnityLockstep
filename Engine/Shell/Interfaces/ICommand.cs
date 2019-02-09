using Lockstep.Network;

namespace Lockstep.Game.Interfaces
{
    public interface ICommand : ISerializable
    {
        ushort Tag { get; }

        void Execute(InputEntity inputEntity);
    }
}